using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Track
{
    public class TrackItem<T> : TrackItem, ITrackError
        where T : INotifyPropertyChanged, ICloneable
    {
        public TrackItems<T> Parent;
        public void Initialise(T original, TrackItems<T> parent = null, Action<TrackItem<T>> validationAction = null, PropertyInfo[] trackProperties = null)
        {
            Parent = parent ?? new TrackItems<T>(new[] { original }, validationAction, trackProperties);
            Reset(original);
            Notify();
        }

        public void Reset(T original)
        {
            Original = original;
            Modified = (T)original?.Clone();
            Confirmed = (T)original?.Clone();
            if (Modified == null) return;
            Modified.PropertyChanged += Modified_PropertyChanged;
        }

        public T Original { get; set; }
        public bool HasChanges => GetHasChanges(Original);
        public T Modified { get; set; }
        public T Confirmed { get; set; }
        public override bool IsModifiedNull => Modified == null;
        public override void OnRefreshErrors() => Parent.ValidationAction?.Invoke(this);
        ~TrackItem()
        {
            if (!IsModifiedNull)
                Modified.PropertyChanged -= Modified_PropertyChanged;
        }
        internal bool GetHasChanges(T item) => Parent.Properties.Any(p => HasChangesPredicate(p, item));
        internal bool HasChangesPredicate(PropertyInfo p, T item)
        {
            var a = IsModifiedNull ? null : p.GetValue(Modified);
            var b = item == null ? null : p.GetValue(item);
            return !a?.Equals(b) ?? b != null;
        }
        private void Modified_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var trackItem in Parent)
                trackItem.Notify(e.PropertyName);
            OnPropertyChanged(nameof(HasChanges));
            Parent.RaiseHasCollectionChanges(this, e.PropertyName);
        }
        public void ResetOriginal(T originalChanged)
        {
            Original = originalChanged;
            OnPropertyChanged(nameof(HasChanges));
        }

        public void AcceptChanges(PropertyInfo[] properties = null)
        {
            foreach (var prop in properties ?? Parent.Properties)
                prop.SetValue(Original, prop.GetValue(Modified));
        }

        protected override void ConfirmChanges(PropertyInfo[] properties = null)
        {
            foreach (var prop in properties ?? Parent.Properties)
                prop.SetValue(Confirmed, prop.GetValue(Modified));
            if (!IsModifiedNull)
                Parent.Notify();
        }
    }
}
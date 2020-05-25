using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Track
{
    public class TrackItem<T> : INotifyPropertyChanged
        where T : INotifyPropertyChanged, ICloneable
    {
        private readonly TrackItems<T> _parent;
        internal T Original;

        public TrackItem(T original, TrackItems<T> parent)
        {
            Original = original;
            _parent = parent;
            Modified = (T)original?.Clone();
            if (Modified != null)
                Modified.PropertyChanged += Modified_PropertyChanged;
        }

        public bool HasChanges => GetHasChanges(Original);
        public T Modified { get; }
        public TrackItems<T> Parent { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        internal bool GetHasChanges(T item) =>
            _parent.Properties.Any(p => HasChangesPredicate(p, item));

        internal bool HasChangesPredicate(PropertyInfo p, T item)
        {
            var a = Modified == null ? null : p.GetValue(Modified);
            var b = item == null ? null : p.GetValue(item);
            return !a?.Equals(b) ?? b != null;
        }

        private void Modified_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasChanges));
            Parent?.RaiseHasCollectionChanges(null, null);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void ResetOriginal(T originalChanged)
        {
            Original = originalChanged;
            OnPropertyChanged(nameof(HasChanges));
        }
    }
}
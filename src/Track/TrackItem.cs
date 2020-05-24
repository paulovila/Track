using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Track
{
    public class TrackItem<T> : INotifyPropertyChanged
        where T : INotifyPropertyChanged, ICloneable
    {
        internal readonly T Original;
        private readonly TrackItems<T> _parent;

        public TrackItem(T original, TrackItems<T> parent)
        {
            Original = original;
            _parent = parent;
            Modified = (T)original?.Clone();
            if (Modified != null)
                Modified.PropertyChanged += Modified_PropertyChanged;
        }
        public bool HasChanges => GetHasChanges(Original);
        internal bool GetHasChanges(T item) => _parent.Properties.Any(p =>
        {
            var a = p.GetValue(Modified);
            var b = p.GetValue(item);
            return !a?.Equals(b) ?? b != null;
        });
        public T Modified { get; }
        public TrackItems<T> Parent { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void Modified_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasChanges));
            Parent?.RaiseHasCollectionChanges(null, null);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
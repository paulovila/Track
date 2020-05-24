using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Track.SampleWpfCore
{
    public class MainWindowViewModel :INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            var items = new[] {new E1 {P1 = "A"}, new E1 {P1 = "B"},};
            Items = items.ToTrackItems();
        }
        private TrackItems<E1> _items;
        public TrackItems<E1> Items
        {
            get => _items;
            set
            {
                _items = value; 
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

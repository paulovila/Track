using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Track.SampleWpfCore
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private TrackItems<E1> _items;
        public MainWindowViewModel()
        {
            ResetCommand = new Command(() => Items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } }
                .ToTrackItems(w => w.IsRequired(q => q.P1)));
            ResetCommand.Execute(null);
            AddCommand = new Command(() => Items.Add(new E1 { P1 = "X" }.ToTrack()));
            RemoveCommand = new Command(() => Items.Remove(Current));
        }
        public TrackItems<E1> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        private TrackItem<E1> _current;
        public TrackItem<E1> Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(); }
        }

        public Command ResetCommand { get; }
        public Command RemoveCommand { get; }
        public Command AddCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
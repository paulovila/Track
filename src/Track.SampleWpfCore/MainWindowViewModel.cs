using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Track.SampleWpfCore
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private TrackItems<E1> _items;
        public MainWindowViewModel()
        {
            ResetCommand = new Command(() => Items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } }.ToTrackItems());
            ResetCommand.Execute(null);
            AddCommand = new Command(() => Items.Add(new E1 {P1 = "X"}.ToTrack()));
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
        public TrackItem<E1> Current { get; set; }
        public Command ResetCommand { get; }
        public Command RemoveCommand { get; }
        public Command AddCommand { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public class Command : ICommand
        {
            private readonly Action _action;
            public Command(Action action) => _action = action;
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _action();
            public event EventHandler CanExecuteChanged;
        }
    }
}
using System;
using System.ComponentModel;

namespace Track.UnitTests
{
    public class E : INotifyPropertyChanged, ICloneable
    {
        private string _p1;
        public event PropertyChangedEventHandler PropertyChanged;

        public string P1
        {
            get => _p1;
            set
            {
                if (_p1 == value) return;
                _p1 = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(P1)));
            }
        }

        public object Clone() => MemberwiseClone();
    }
}
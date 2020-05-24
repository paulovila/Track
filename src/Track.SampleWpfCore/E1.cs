using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Track.SampleWpfCore
{
    public class E1 : INotifyPropertyChanged, ICloneable
    {
        private string _p1;
        public string P1 { get => _p1; set { _p1 = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public object Clone() => MemberwiseClone();
    }
}
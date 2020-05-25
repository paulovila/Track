using System;
using System.ComponentModel;

namespace Track
{
    public class TrackItemEvent<T> : EventArgs where T : INotifyPropertyChanged, ICloneable
    {
        public TrackItem<T> Item { get; set; }
        public string PropertyNameChanged { get; set; }
    }
}
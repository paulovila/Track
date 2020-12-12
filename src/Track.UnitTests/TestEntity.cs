using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Track.UnitTests
{
    public sealed class TestEntity : INotifyPropertyChanged, ICloneable
    {
        private decimal _decimalId;
        private int _intId;
        private string _name;
        private string _name2;
        private IEnumerable _items;
        private ICollection<int> _items2;

        public int IntId
        {
            get => _intId;
            set
            {
                _intId = value;
                OnPropertyChanged();
            }
        }

        public decimal DecimalId
        {
            get => _decimalId;
            set
            {
                _decimalId = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public string Name2
        {
            get => _name2;
            set
            {
                _name2 = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ICollection<int> Items2
        {
            get => _items2;
            set
            {
                _items2 = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string propertyName) => OnPropertyChanged(propertyName);

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public object Clone() => MemberwiseClone();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Track
{
    public class TrackItems<T> : ObservableCollection<TrackItem<T>>, ITrackError
        where T : INotifyPropertyChanged, ICloneable
    {
        public PropertyInfo[] Properties { get; }
        private T[] _originalItems;
        public Action<TrackItem<T>> ValidationAction { get; set; }

        public TrackItems(T[] items, Action<TrackItem<T>> validationAction, PropertyInfo[] trackProperties)
        {
            _originalItems = items;
            ValidationAction = validationAction;
            Properties = trackProperties ?? typeof(T).GetProperties()
                .Where(w => w.SetMethod != null).ToArray();

            CompareFunc = (a, b) => EqualityComparer<T>.Default.Equals(a, b);
        }

        private bool _init;
        public void InitCollectionChanged()
        {
            foreach (var item in _originalItems)
            {
                var ti = new TrackItem<T>();
                ti.Initialise(item, this);
                base.Add(ti);
            }
            CollectionChanged += TrackItems_CollectionChanged;
            _init = true;
            RaiseHasCollectionChanges(null, null);
        }

        public Func<T, T, bool> CompareFunc { get; set; }

        public bool HasCollectionChanges => this.Where(w => w.Original != null && _originalItems.Contains(w.Original))
                                                .Any(i => i.HasChanges)
                                            ||
                                            Count != _originalItems.Length
                                            ||
                                            ItemsChanged();

        private bool ItemsChanged()
        {
            var originalsChanged = _originalItems.Where(w => this.All(g => !CompareFunc(g.Original, w))).ToList();
            var trackItemsChanged = this.Where(w => w != null && _originalItems.All(q => !CompareFunc(q, w.Original)))
                .ToList();
            foreach (var originalChanged in originalsChanged)
            {
                var similarTrackItemChanged = trackItemsChanged.FirstOrDefault(w => !w.GetHasChanges(originalChanged));
                if (similarTrackItemChanged != null)
                {
                    similarTrackItemChanged.ResetOriginal(originalChanged);
                    trackItemsChanged.Remove(similarTrackItemChanged);
                }
            }
            return trackItemsChanged.Any();
        }
        public void RaiseHasCollectionChanges(TrackItem item, string propertyName)
        {
            if (propertyName != null && Properties?.All(p => p.Name != propertyName) == true)
                return;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasCollectionChanges)));
            if (item != null && propertyName != null)
            {
                ItemPropertyChanged?.Invoke(this, new TrackItemEvent<T> { Item = item as TrackItem<T>, PropertyNameChanged = propertyName });
                ErrorsChanged?.Invoke(item, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public EventHandler<TrackItemEvent<T>> ItemPropertyChanged { get; set; }

        public new void Add(TrackItem<T> trackItem)
        {
            trackItem.Parent = this;
            base.Add(trackItem);
        }

        public new void Remove(TrackItem<T> trackItem)
        {
            base.Remove(trackItem);
            foreach (var item in this)
                item.Notify();
            Notify();
        }
        public int ModifiedPropertiesCount() =>
            this
                .Where(w => w != null && w.HasChanges)
                .Sum(trackObject =>
                    Properties
                        .Count(propertyInfo => trackObject.HasChangesPredicate(propertyInfo, trackObject.Original)));

        public ObservableCollection<T> GetCollection() => new ObservableCollection<T>(this.Select(w => w.Modified).ToList());
        public IEnumerable GetErrors(string propertyName) => this.SelectMany(w => w.GetValidations(propertyName)).Where(w => w != null).ToArray();
        public bool HasErrors => this.Any(w => w.HasErrors);
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasChanges => this.Any(w => w.HasChanges);
        public string FirstError => this.FirstOrDefault(w => w.HasErrors)?.FirstError;

        public void Notify()
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(""));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasChanges)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasCollectionChanges)));
        }

        public void Refresh(IEnumerable<T> items)
        {
            Clear();
            _originalItems = items.ToArray();
            InitCollectionChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public TrackItem<T> AddItem(T item, int position = 0)
        {
            var ti = new TrackItem<T> { Parent = this };
            ti.Reset(item);
            InsertItem(position, ti);
            foreach (var trackItem in this)
                trackItem.Notify();
            Notify();
            return ti;
        }
        private void TrackItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => RaiseHasCollectionChanges(null, null);

        public void Disconnect()
        {
            if (_init)
            {
                CollectionChanged -= TrackItems_CollectionChanged;
                _init = false;
            }
            Clear();
        }
    }
}
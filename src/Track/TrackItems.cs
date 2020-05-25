using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Track
{
    public sealed class TrackItems<T> : ObservableCollection<TrackItem<T>> where T : INotifyPropertyChanged, ICloneable
    {
        internal readonly PropertyInfo[] Properties;
        private readonly T[] _originalItems;

        public TrackItems(T[] items, PropertyInfo[] trackProperties)
        {
            _originalItems = items;
            Properties = trackProperties ?? typeof(T).GetProperties()
                .Where(w => w.SetMethod != null).ToArray();

            foreach (var item in items)
                Add(new TrackItem<T>(item, this) { Parent = this });
            CollectionChanged += (s, e) => RaiseHasCollectionChanges(null, null);
        }

        public bool HasCollectionChanges => this.Where(w => w.Original != null && _originalItems.Contains(w.Original))
                                                .Any(i => i.HasChanges)
                                            ||
                                            Count != _originalItems.Length
                                            ||
                                            ItemsChanged();

        private bool ItemsChanged()
        {
            var originalsChanged = _originalItems.Where(w => !this.Select(g => g.Original).Contains(w)).ToList();
            var trackItemsChanged = this.Where(w => w != null && !_originalItems.Contains(w.Original)).ToList();
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
        public void RaiseHasCollectionChanges(TrackItem<T> item, string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasCollectionChanges)));
            if (item != null && propertyName != null)
                ItemPropertyChanged?.Invoke(this, new TrackItemEvent<T> { Item = item, PropertyNameChanged = propertyName });
        }
        public EventHandler<TrackItemEvent<T>> ItemPropertyChanged { get; set; }

        public new void Add(TrackItem<T> item)
        {
            item.Parent = this;
            base.Add(item);
        }
        public int ModifiedPropertiesCount() =>
            this
                .Where(w => w != null && w.HasChanges)
                .Sum(trackObject =>
                    Properties
                        .Count(propertyInfo => trackObject.HasChangesPredicate(propertyInfo, trackObject.Original)));

        public ObservableCollection<T> GetCollection() => new ObservableCollection<T>(this.Select(w => w.Modified).ToList());
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Track
{
    public sealed class TrackItems<T> : ObservableCollection<TrackItem<T>> where T : INotifyPropertyChanged, ICloneable
    {
        internal readonly PropertyInfo[] Properties;
        private readonly T[] _originalItems;

        public TrackItems(T[] items, Expression<Func<T, object>>[] trackPropertyExpressions)
        {
            var l = new List<PropertyInfo>();
            _originalItems = items;
            if (trackPropertyExpressions != null)
                l.AddRange(trackPropertyExpressions
                    .Select(propertyExpression => propertyExpression.Body as MemberExpression)
                    .Select(expression => expression.Member as PropertyInfo));
            else
                l.AddRange(typeof(T).GetProperties()
                    .Where(w => w.SetMethod != null));
            Properties = l.ToArray();

            foreach (var item in items)
                Add(new TrackItem<T>(item, this) { Parent = this });
            CollectionChanged += RaiseHasCollectionChanges;
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
        public void RaiseHasCollectionChanges(object sender, NotifyCollectionChangedEventArgs e) =>
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasCollectionChanges)));

        public new void Add(TrackItem<T> item)
        {
            item.Parent = this;
            base.Add(item);
        }
    }
}
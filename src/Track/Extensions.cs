using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Track
{
    public static class Extensions
    {
        public static TrackItem<T> ToTrack<T>(this T item,
            Expression<Func<T, object>>[] trackPropertyExpressions = null)
            where T : INotifyPropertyChanged, ICloneable =>
            new TrackItem<T>(item, new TrackItems<T>(new[] { item }, trackPropertyExpressions));

        public static TrackItems<T> ToTrackItems<T>(this IEnumerable<T> items,
            Expression<Func<T, object>>[] trackPropertyExpressions = null)
            where T : INotifyPropertyChanged, ICloneable =>
            new TrackItems<T>(items.ToArray(), trackPropertyExpressions);
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Track
{
    public static class Extensions
    {
        public static TrackItem<T> ToTrack<T>(this T item,
            PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable =>
            new TrackItem<T>(item, new TrackItems<T>(new[] { item }, trackProperties));

        public static TrackItems<T> ToTrackItems<T>(this IEnumerable<T> items,
            PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable =>
            new TrackItems<T>(items.ToArray(), trackProperties);
    }
}
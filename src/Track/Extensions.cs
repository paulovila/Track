using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Track
{
    public static class Extensions
    {
        public static TrackItem<T> ToTrack<T>(this T item, Action<TrackItem<T>> validationAction = null, PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable
        {
            var ti = new TrackItem<T>();
            ti.Initialise(item, validationAction, null, trackProperties);
            return ti;
        }

        public static TrackItems<T> ToTrackItems<T>(this IEnumerable<T> items, Action<TrackItem<T>> validationAction = null,
            PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable =>
            new TrackItems<T>(items.ToArray(), validationAction, trackProperties);
        public static string Beautify(this string path)
        {
            var text = path;
            if (path.Contains("."))
                text = path.Split('.').Last();
            return text.CapitalsIntoWords();
        }
        public static string CapitalsIntoWords(this string text) => string.Join(" ", Regex.Split(text, @"(?<!^)(?=[A-Z])"));

        public static string GetPropertyName<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            var p = expression.Body is UnaryExpression body
                ? (body.Operand is MemberExpression operand ? operand.Member : null) as PropertyInfo
                : (expression.Body is MemberExpression body1 ? body1.Member : null) as PropertyInfo;
            return p?.Name ?? expression.ToString();
        }
    }
}
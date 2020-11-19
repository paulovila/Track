using System;
using System.Collections;
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
        public static TrackItem<T> ToTrack<T>(this T item, Action<TrackItem<T>> validationAction = null, TrackItems<T> parent = null, PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable
        {
            var ti = new TrackItem<T>();
            ti.Initialise(item, parent, validationAction, trackProperties);
            ti.Parent.Add(ti);
            return ti;
        }

        public static TrackItems<T> ToTrackItems<T>(this IEnumerable<T> items, Action<TrackItem<T>> validationAction = null,
            PropertyInfo[] trackProperties = null)
            where T : INotifyPropertyChanged, ICloneable
        {
            var t = new TrackItems<T>(items.ToArray(), validationAction, trackProperties);
            t.InitCollectionChanged();
            return t;
        }

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
        public static void IsRequired<T>(this TrackItem<T> t, Expression<Func<T, object>> expression) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, value == null || value is string s && string.IsNullOrEmpty(s), "{0} is required", path.Beautify());
        }
        public static void IsNotNegative<T>(this TrackItem<T> t, Expression<Func<T, int>> expression) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, value < 0, "{0} should be not negative", path.Beautify());
        }
        public static void IsNotNegative<T>(this TrackItem<T> t, Expression<Func<T, decimal>> expression) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, value < 0, "{0} should be not negative", path.Beautify());
        }
        public static void IsPositiveMessage<T>(this TrackItem<T> t, Expression<Func<T, int>> expression, string message) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, value <= 0, message);
        }
        public static void HasItemsMessage<T>(this TrackItem<T> t, Expression<Func<T, IEnumerable>> expression, string message) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, value == null || !value.GetEnumerator().MoveNext(), message);
        }
        public static void IsRequiredMessage<T>(this TrackItem<T> t, Expression<Func<T, string>> expression, string message) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, string.IsNullOrEmpty(value), message);
        }
        public static void IsUpperCase<T>(this TrackItem<T> t, Expression<Func<T, string>> expression, string message) where T : INotifyPropertyChanged, ICloneable
        {
            var (path, value) = PathValue(expression, t);
            t.UpdateError(path, !string.IsNullOrEmpty(value) && value.ToUpperInvariant() != value, message);
        }

        private static (string, TItem) PathValue<T, TItem>(Expression<Func<T, TItem>> expression, TrackItem<T> ti) where T : INotifyPropertyChanged, ICloneable => (expression.GetPropertyName(), expression.Compile()(ti.Modified));
    }
}
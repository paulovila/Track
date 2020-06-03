using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Track
{
    public abstract class TrackItem : INotifyPropertyChanged
    {
        private readonly Dictionary<string, (string, object[])> _errors;
        private bool _hasErrors;

        public TrackItem() => _errors = new Dictionary<string, (string, object[])>();

        public bool HasErrors
        {
            get => _hasErrors;
            protected set
            {
                SetProperty(ref _hasErrors, value);
                OnPropertyChanged(nameof(FirstError));
            }
        }

        public string FirstError => Validations.FirstOrDefault();
        public IEnumerable<string> Validations => _errors.Values.Select(er => string.Format(er.Item1, er.Item2));
        public abstract bool IsModifiedNull { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;
            storage = value;
            OnPropertyChanged(propertyName);
        }

        public abstract void OnRefreshErrors();
        public void UpdateError(string path, bool isError, string template, params object[] pars)
        {
            var key = $"{path}#{template}";
            if (isError)
                _errors[key] = (template, pars);
            else if (_errors.ContainsKey(key))
                _errors.Remove(key);
        }

        internal bool IsNullOrEmpty(object obj) => obj == null || obj is string s && string.IsNullOrEmpty(s);
        public void Notify()
        {
            if (IsModifiedNull)
                _errors.Clear();
            else
                OnRefreshErrors();
            HasErrors = IsModifiedNull || _errors.Any();
            OnPropertyChanged(nameof(Validations));
        }

        public IEnumerable<string> GetValidations(string path)
        {
            var prefix = path + "#";
            return _errors.Keys
                .Where(w => w.StartsWith(prefix))
                .Select(key =>
                {
                    var (item1, objects) = _errors[key];
                    return string.Format(item1, objects);
                });
        }
    }

    public class TrackItem<T> : TrackItem
        where T : INotifyPropertyChanged, ICloneable
    {
        internal TrackItems<T> Parent;

        public TrackItem(T original, TrackItems<T> parent)
        {
            Parent = parent;
            Original = original;
            Modified = (T)original?.Clone();
            if (Modified == null) return;
            Modified.PropertyChanged += Modified_PropertyChanged;
            Notify();
        }

        public T Original { get; set; }
        public bool HasChanges => GetHasChanges(Original);
        public T Modified { get; }
        public override bool IsModifiedNull => Modified == null;
        public override void OnRefreshErrors() => Parent.ValidationAction?.Invoke(this);
        ~TrackItem()
        {
            if (!IsModifiedNull)
                Modified.PropertyChanged -= Modified_PropertyChanged;
        }
        internal bool GetHasChanges(T item) => Parent.Properties.Any(p => HasChangesPredicate(p, item));
        internal bool HasChangesPredicate(PropertyInfo p, T item)
        {
            var a = IsModifiedNull ? null : p.GetValue(Modified);
            var b = item == null ? null : p.GetValue(item);
            return !a?.Equals(b) ?? b != null;
        }
        private void Modified_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasChanges));
            Parent.RaiseHasCollectionChanges(this, e.PropertyName);
            Notify();
        }
        public void ResetOriginal(T originalChanged)
        {
            Original = originalChanged;
            OnPropertyChanged(nameof(HasChanges));
        }
        public void IsRequired(Expression<Func<T, object>> expression)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, IsNullOrEmpty(value), "{0} is required", path.Beautify());
        }
        public void IsNotNegative(Expression<Func<T, int>> expression)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, value < 0, "{0} should be not negative", path.Beautify());
        }
        public void IsRequiredMessage(Expression<Func<T, string>> expression, string message)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, string.IsNullOrEmpty(value), message);
        }
        public void IsUpperCase(Expression<Func<T, string>> expression, string message)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, !string.IsNullOrEmpty(value) && value.ToUpperInvariant() != value, message);
        }
        public void HasAtLeastItems<TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, int i)
        {
            var (path, value) = PathValue(expression);
            var entityName = typeof(TItem).Name;
            var verb = i == 1 ? "is" : "are";
            UpdateError(path, value.Count() < i, "At least {0} {1} {2} needed", i, entityName,
                verb);
        }
        public void HasItemsMessage(Expression<Func<T, IEnumerable>> expression, string message)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, value == null || !value.GetEnumerator().MoveNext(), message);
        }
        public void IsPositiveMessage(Expression<Func<T, int>> expression, string message)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, value <= 0, message);
        }
        public void IsNotNegative(Expression<Func<T, decimal>> expression)
        {
            var (path, value) = PathValue(expression);
            UpdateError(path, value < 0, "{0} should be not negative", path.Beautify());
        }
        private (string, TItem) PathValue<TItem>(Expression<Func<T, TItem>> expression) => (expression.GetPropertyName(), expression.Compile()(Modified));

    }
}
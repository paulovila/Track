using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Track
{
    public abstract class Validate<T> : Validate where T : INotifyPropertyChanged, ICloneable
    {
        public void Notify()
        {
            if (_item == null)
                _errors.Clear();
            else
                OnRefreshErrors();
            HasValidations = _item == null || _errors.Any();
            OnPropertyChanged(nameof(Validations));
        }
        private readonly Dictionary<string, (string, object[])> _errors;
        private TrackItem<T> _item;
        public Validate() => _errors = new Dictionary<string, (string, object[])>();

        public TrackItem<T> Item
        {
            get => _item;
            set
            {
                if (_item != null && _item.Modified != null)
                    _item.Modified.PropertyChanged -= OnItemPropertyChanged;

                _item = value;

                if (_item != null && _item.Modified != null)
                {
                    _item.PropertyChanged += OnItemPropertyChanged;
                }
            }
        }


        public override IEnumerable<string> Validations =>
            _errors.Values.Select(er => string.Format(er.Item1, er.Item2));

        ~Validate()
        {
            if (_item != null && _item.Modified != null)
                _item.Modified.PropertyChanged -= OnItemPropertyChanged;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => Notify();

        public abstract void OnRefreshErrors();

        public void UpdateError(string path, bool isError, string template, params object[] pars)
        {
            var key = $"{path}#{template}";
            if (isError)
            {
                _errors[key] = (template, pars);
            }
            else
            {
                if (_errors.ContainsKey(key))
                    _errors.Remove(key);
            }
        }

        public void IsRequired(Expression<Func<T, object>> propertyFunc)
        {
            var (path, val) = GetPathValue(propertyFunc);
            UpdateError(path, IsNullOrEmpty(val), "{0} is required", path.Beautify());
        }

        public void HasAtLeastItems<TItem>(Expression<Func<T, IEnumerable<TItem>>> itemsFunc, int i)
        {
            var path = itemsFunc.Body.ToString();
            var entityName = typeof(TItem).Name;
            var verb = i == 1 ? "is" : "are";
            UpdateError(path, itemsFunc.Compile()(Item.Modified).Count() < i, "At least {0} {1} {2} needed", i, entityName,
                verb);
        }

        private bool IsNullOrEmpty(object obj) => obj == null || obj is string s && string.IsNullOrEmpty(s);

        private (string, object) GetPathValue(Expression<Func<T, object>> expression)
        {
            var propertyInfo = expression.Body is UnaryExpression body ?
                (body.Operand is MemberExpression operand ? operand.Member : null) as PropertyInfo
                :
                (expression.Body is MemberExpression body1 ? body1.Member : null) as PropertyInfo;
            return (propertyInfo?.Name ?? expression.ToString(), expression.Compile()(Item.Modified));
        }
    }

    public abstract class Validate : INotifyPropertyChanged
    {
        private string _errorMessage;
        private bool _hasValidations;
        private string _okMessage;

        public string OkMessage
        {
            get => _okMessage;
            set => SetProperty(ref _okMessage, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasValidations
        {
            get => _hasValidations;
            protected set
            {
                SetProperty(ref _hasValidations, value);
                OnPropertyChanged(nameof(FirstValidation));
            }
        }

        protected void SetProperty<T>(ref T storage, T value, string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return;
            storage = value;
            OnPropertyChanged(propertyName);
        }

        public string FirstValidation => Validations.FirstOrDefault();

        public abstract IEnumerable<string> Validations { get; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
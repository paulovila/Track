﻿using System;
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
        private bool _hasValidations;

        public TrackItem()
        {
            _errors = new Dictionary<string, (string, object[])>();
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

        public string FirstValidation => Validations.FirstOrDefault();
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
            HasValidations = IsModifiedNull || _errors.Any();
            OnPropertyChanged(nameof(Validations));
        }
    }

    public  class TrackItem<T> : TrackItem
        where T : INotifyPropertyChanged, ICloneable
    {
        internal  TrackItems<T> _parent;

        public TrackItem(T original, TrackItems<T> parent)
        {
            _parent = parent;
            Original = original;
            Modified = (T) original?.Clone();
            if (Modified == null) return;
            Modified.PropertyChanged += Modified_PropertyChanged;
            Notify();
        }

        public T Original { get; set; }
        public bool HasChanges => GetHasChanges(Original);
        public T Modified { get; }
        public override bool IsModifiedNull => Modified == null;
        public override void OnRefreshErrors() => _parent.ValidationAction?.Invoke(this);
        ~TrackItem()
        {
            if (!IsModifiedNull)
                Modified.PropertyChanged -= Modified_PropertyChanged;
        }

        internal bool GetHasChanges(T item)
        {
            return _parent.Properties.Any(p => HasChangesPredicate(p, item));
        }

        internal bool HasChangesPredicate(PropertyInfo p, T item)
        {
            var a = IsModifiedNull ? null : p.GetValue(Modified);
            var b = item == null ? null : p.GetValue(item);
            return !a?.Equals(b) ?? b != null;
        }

        private void Modified_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasChanges));
            _parent.RaiseHasCollectionChanges(this, e.PropertyName);
            Notify();
        }

        public void ResetOriginal(T originalChanged)
        {
            Original = originalChanged;
            OnPropertyChanged(nameof(HasChanges));
        }

        public void IsRequired(Expression<Func<T, object>> propertyFunc)
        {
            var path = propertyFunc.GetPropertyName();
            UpdateError(path, IsNullOrEmpty(propertyFunc.Compile()(Modified)), "{0} is required", path.Beautify());
        }
        public void IsNotNegative(Expression<Func<T, int>> propertyFunc)
        {
            var path = propertyFunc.GetPropertyName();
            UpdateError(path, propertyFunc.Compile()(Modified) < 0, "{0} should be not negative", path.Beautify());
        }

        public void HasAtLeastItems<TItem>(Expression<Func<T, IEnumerable<TItem>>> itemsFunc, int i)
        {
            var path = itemsFunc.Body.ToString();
            var entityName = typeof(TItem).Name;
            var verb = i == 1 ? "is" : "are";
            UpdateError(path, itemsFunc.Compile()(Modified).Count() < i, "At least {0} {1} {2} needed", i, entityName,
                verb);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Track
{
    public abstract class TrackItem : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, (string, object[])> _errors = new Dictionary<string, (string, object[])>();

        private readonly Dictionary<string, (PropertyInfo[], string, object[])> _warnings =
            new Dictionary<string, (PropertyInfo[], string, object[])>();

        private bool _hasErrors;
        public string FirstError => Validations.FirstOrDefault();
        public IEnumerable<string> Validations => _errors.Values.Select(er => string.Format(er.Item1, er.Item2));
        public abstract bool IsModifiedNull { get; }
        public IEnumerable GetErrors(string propertyName) => GetValidations(propertyName);

        public bool HasErrors
        {
            get => _hasErrors;
            protected set
            {
                SetProperty(ref _hasErrors, value, nameof(HasErrors));
                OnPropertyChanged(nameof(FirstError));
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseError(string propertyName) =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            RaiseError(propertyName);
        }

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
            var key = $"Modified.{path}#{template}";
            if (isError)
                _errors[key] = (template, pars);
            else if (_errors.ContainsKey(key))
                _errors.Remove(key);
        }

        public void UpdateWarn(string path, PropertyInfo[] properties, bool isWarning, string template, params object[] pars)
        {
            var key = $"Modified.{path}#{template}";
            if (isWarning)
                _warnings[key] = (properties, template, pars);
            else if (_warnings.ContainsKey(key))
                _warnings.Remove(key);
        }

        public void Notify(string propertyName = null)
        {
            if (IsModifiedNull)
                _errors.Clear();
            else
                OnRefreshErrors();
            HasErrors = IsModifiedNull || _errors.Any();
            OnPropertyChanged(nameof(Validations));
            RaiseError(propertyName);
        }

        public IEnumerable<TrackResult> GetValidations(string path)
        {
            bool Filter(string p) => string.IsNullOrEmpty(path) || p.StartsWith(path + "#");
            return _errors
                .Keys
                .Where(Filter)
                .Select(key => new TrackResult { Message = string.Format(_errors[key].Item1, _errors[key].Item2) })
                .Union(_warnings.Where(w => Filter(w.Key))
                    .Select(k => new TrackResult
                    {
                        Message = string.Format(k.Value.Item2, k.Value.Item3),
                        ConfirmAction = () =>
                        {
                            if (_warnings.ContainsKey(k.Key))
                                _warnings.Remove(k.Key);
                            ConfirmChanges(k.Value.Item1);
                        }
                    }))
                .ToArray();
        }

        protected abstract void ConfirmChanges(PropertyInfo[] properties = null);
    }
}
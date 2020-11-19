using System.ComponentModel;

namespace Track
{
    public interface ITrackError : INotifyDataErrorInfo
    {
        bool HasChanges { get; }
        string FirstError { get; }
        void Notify(string propertyName);
    }
}
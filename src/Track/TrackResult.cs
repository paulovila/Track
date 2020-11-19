using System;

namespace Track
{
    public class TrackResult
    {
        public Action ConfirmAction { get; set; }
        public string Message { get; set; }
        public override string ToString() => Message;
    }
}
using System;

namespace ColMusCa
{
    // Define a class to hold custom event info
    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs(string s)
        {
            message = s;
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
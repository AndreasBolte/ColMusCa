using System;

namespace ColMusCa
{
    public class Project
    {
        private string name;
        private int pageCounter;
        private int pageIndex;
        private string pathName;

        public Project()
        {
            PageIndex = 0;
            PageCounter = 0;
        }

        // Declare the event using EventHandler<T>
        public event EventHandler<CustomEventArgs> NameChangedEvent;

        public event EventHandler<CustomEventArgs> PageIndexChangedEvent;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnNameChangedEvent(new CustomEventArgs(this.Name));
            }
        }

        public int PageCounter { get => pageCounter; set => pageCounter = value; }

        public int PageIndex
        {
            get
            {
                return pageIndex;
            }
            set
            {
                pageIndex = value;
                OnPageIndexEvent(new CustomEventArgs(this.PageIndex.ToString()));
            }
        }

        public string PathName { get => pathName; set => pathName = value; }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnNameChangedEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = NameChangedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        protected virtual void OnPageIndexEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> handler = PageIndexChangedEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += String.Format(" at {0}", DateTime.Now.ToString());

                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
    }
}
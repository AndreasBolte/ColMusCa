using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColMusCa
{
    /// <summary>
    /// Object Binding for the Farbfilter Window
    /// </summary>
    public class WindowBindings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void SetProperty<T>(ref T storage, T value, [CallerMemberName] string property = null)
        {
            storage = value;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private double _ProgressBarThink;

        /// <summary>
        /// Source for Binding for ProgressBarThink: value in the Farbfilter Window
        /// </summary>
        public double ProgressBarThink
        {
            get { return _ProgressBarThink; }
            set { SetProperty(ref _ProgressBarThink, value); }
        }

        private string _LabelProgress;

        /// <summary>
        /// Source for Binding for LabelProgress in the Farbfilter Window
        /// </summary>
        public string LabelProgress
        {
            get { return _LabelProgress; }
            set { SetProperty(ref _LabelProgress, value); }
        }
    }
}
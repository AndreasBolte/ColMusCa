using System;
using System.Windows;
using System.Windows.Input;

namespace ColMusCa
{
    public class ProjectSaveCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBox.Show("The " + "ProjectSaveCommand" + " command has the parameter Null");
        }
    }
}
﻿using AndromedaCore.Managers;
using AndromedaCore.ViewModel;

namespace Andromeda.ViewModel
{
    public class ResultConsoleViewModel : ViewModelBase
    {
        private string _consoleString;
        public string ConsoleString
        {
            get => _consoleString;
            set
            {
                _consoleString = value;
                OnPropertyChanged();
            }
        }

        public ResultConsoleViewModel()
        {
            ResultConsole.ConsoleChange += UpdateConsoleData;
            ResultConsole.Instance.OnConsoleChange(ResultConsole.Instance.ConsoleString);
        }

        public void UpdateConsoleData(string updateString)
        {
            ConsoleString = updateString;
        }
    }
}
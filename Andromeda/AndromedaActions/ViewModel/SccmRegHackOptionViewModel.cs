﻿using System;
using System.Windows.Input;
using AndromedaCore.Infrastructure;
using AndromedaCore.ViewModel;

namespace AndromedaActions.ViewModel
{
    public class SccmRegHackOptionViewModel : RequestCloseViewModel
    {

        private bool _remoteAccessEnabled = true;
        private bool _requiresUserApproval = true;
        private bool _showConnectionBanner = true;
        private bool _showTaskBarIcon = true;
        private bool _allowAccessOnUnattended = true;
        private bool _allowLocalAdministratorsToRemoteControl = true;

        public bool RemoteAccessEnabled
        {
            get => _remoteAccessEnabled;
            set
            {
                _remoteAccessEnabled = value;
                OnPropertyChanged("RemoteAccessEnabled");
            }
        }

        public bool RequiresUserApproval
        {
            get => _requiresUserApproval;
            set
            {
                _requiresUserApproval = value;
                OnPropertyChanged("RequiresUserApproval");
            }
        }

        public bool ShowConnectionBanner
        {
            get => _showConnectionBanner;
            set
            {
                _showConnectionBanner = value;
                OnPropertyChanged("ShowConnectionBanner");
            }
        }

        public bool ShowTaskbarIcon
        {
            get => _showTaskBarIcon;
            set
            {
                _showTaskBarIcon = value;
                OnPropertyChanged("ShowTaskbarIcon");
            }
        }

        public bool AllowAccessOnUnattended
        {
            get => _allowAccessOnUnattended;
            set
            {
                _allowAccessOnUnattended = value;
                OnPropertyChanged("AllowAccessOnUnattended");
            }
        }

        public bool AllowLocalAdministratorsToRemoteControl
        {
            get => _allowLocalAdministratorsToRemoteControl;
            set
            {
                _allowLocalAdministratorsToRemoteControl = value;
                OnPropertyChanged("AllowLocalAdministratorsToRemoteControl");
            }
        }

        private ICommand _okayCmd;
        public ICommand OkayCommand
        {
            get
            {
                if (_okayCmd == null)
                {
                    _okayCmd = new DelegateCommand(param => OkayClose(), param => true);
                }
                return _okayCmd;
            }
        }

        private ICommand _cancelCmd;
        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new DelegateCommand(param => CancelClose(), param => true);
                }
                return _cancelCmd;
            }
        }

        private ICommand _forceAccessSettingsCommand;
        public ICommand ForceAccessSettingsCommand
        {
            get
            {
                if (_forceAccessSettingsCommand == null)
                {
                    _forceAccessSettingsCommand = new DelegateCommand(param => SetForceAccessSettings(), param => true);
                }
                return _forceAccessSettingsCommand;
            }
        }

        private ICommand _silentSettingsCommand;
        public ICommand SilentSettingsCommand
        {
            get
            {
                if (_silentSettingsCommand == null)
                {
                    _silentSettingsCommand = new DelegateCommand(param => SetSilentSettings(), param => true);
                }
                return _silentSettingsCommand;
            }
        }

        private ICommand _defaultSettingsCommand;
        public ICommand DefaultSettingsCommand
        {
            get
            {
                if (_defaultSettingsCommand == null)
                {
                    _defaultSettingsCommand = new DelegateCommand(param => SetDefaultSettings(), param => true);
                }
                return _defaultSettingsCommand;
            }
        }

        private bool _result;
        public bool Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged("Result");
            }
        }

        public void OkayClose()
        {
            _result = true;
            OnRequestClose(EventArgs.Empty);
        }

        public void CancelClose()
        {
            _result = false;
            OnRequestClose(EventArgs.Empty);
        }

        private void SetDefaultSettings()
        {
            RemoteAccessEnabled = true;
            RequiresUserApproval = true;
            ShowConnectionBanner = true;
            ShowTaskbarIcon = true;
            AllowAccessOnUnattended = true;
            AllowLocalAdministratorsToRemoteControl = true;
        }

        private void SetSilentSettings()
        {
            RemoteAccessEnabled = true;
            RequiresUserApproval = false;
            ShowConnectionBanner = false;
            ShowTaskbarIcon = false;
            AllowAccessOnUnattended = true;
            AllowLocalAdministratorsToRemoteControl = true;
        }

        private void SetForceAccessSettings()
        {
            RemoteAccessEnabled = true;
            RequiresUserApproval = false;
            ShowConnectionBanner = true;
            ShowTaskbarIcon = true;
            AllowAccessOnUnattended = true;
            AllowLocalAdministratorsToRemoteControl = true;
        }
    }
}
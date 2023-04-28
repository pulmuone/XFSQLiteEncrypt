using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XFSQLiteEncrypt.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public ICommand SortedCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModelBase()
        {

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null, Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public async Task<bool> CheckConnectivity(string title, string message)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                return true;

            await DisplayAlert(title, message);
            return false;
        }

        bool isBusy = false;
        bool isEnabled = true;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        public bool IsNotBusy => !IsBusy;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { SetProperty(ref isEnabled, value); }
        }

        string title = string.Empty;
        /// <summary>
        /// Public property to set and get the title of the item
        /// </summary>
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private bool _isControlEnable = true;
        public bool IsControlEnable
        {
            get => _isControlEnable;
            set => SetProperty(ref this._isControlEnable, value);
        }

        public Task DisplayAlert(string title, string message) =>
            Application.Current.MainPage.DisplayAlert(title, message, "OK");
        public Task<bool> DisplayAlert(string title, string message, string accept, string cancel) =>
            Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
    }
}

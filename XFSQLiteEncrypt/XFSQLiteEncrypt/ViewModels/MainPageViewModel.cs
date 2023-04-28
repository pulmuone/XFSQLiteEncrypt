using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XFSQLiteEncrypt.Data;
using XFSQLiteEncrypt.Models;

namespace XFSQLiteEncrypt.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand SaveCommand { get; private set; }
        public ICommand DbFileExportCommand { get; private set; }

        private string _userId;
        private string _userName;
        private string _password;

        public string UserId
        {
            get => _userId;
            set => SetProperty(ref this._userId, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref this._userName, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref this._password, value);
        }

        public MainPageViewModel()
        {
            SaveCommand = new Command(async () => await Save(), () => IsControlEnable);
            DbFileExportCommand = new Command(async () => await DbFileExport(), () => IsControlEnable);
        }

        private async Task Save()
        {
            IsControlEnable = false;
            IsBusy = true;
            (SaveCommand as Command).ChangeCanExecute();

            User user = new User()
            {
                UserId = this.UserId,
                UserName = this.UserName,
                Password = this.Password
            };

            var result = await Db.Instance.UserInsert2(user);

            Debug.WriteLine(result);

            IsControlEnable = true;
            IsBusy = false;
            (SaveCommand as Command).ChangeCanExecute();
        }

        private async Task DbFileExport()
        {
            IsControlEnable = false;
            IsBusy = true;
            (DbFileExportCommand as Command).ChangeCanExecute();

            var path = Db.Instance.DBToExport();
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = "DB File",
                        File = new ShareFile(path)
                    });
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine("ERROR: " + ex.Message);
                    await Application.Current.MainPage.DisplayAlert("Err", ex.Message, "OK");
                }
            }

            IsControlEnable = true;
            IsBusy = false;
            (DbFileExportCommand as Command).ChangeCanExecute();
        }
    }
}

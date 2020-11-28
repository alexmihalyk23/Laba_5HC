using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using FluentFTP;
using HandyControl.Controls;
using Laba_5HC.Commands;
using Laba_5HC.Views;
using Microsoft.Win32;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace Laba_5HC.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private string _host;
        private string _user;
        private string _password;
        private int _port = 21;
        private string _uploadFile;
        private int _selectedServerIndex;
        private int _selectedClientIndex;
        private string file;
        private string pathDir;

        private int _currentProgress;

        private ObservableCollection<string> _serverDir = new ObservableCollection<string>();
        private ObservableCollection<string> _clientDir = new ObservableCollection<string>();
        public FtpClient client;

        public int SelectedServerIndex
        {
            get
            {
                return _selectedServerIndex;
            }

            set
            {
                if (_selectedServerIndex == value)
                {
                    return;
                }

                // At this point _selectedIndex is the old selected item index

                _selectedServerIndex = value;

                // At this point _selectedIndex is the new selected item index

                OnPropertyChanged("SelectedServerIndex");
            }
        }

        public string PathDir
        {
            get
            {
                return pathDir;
            }
            set
            {
                if(pathDir == value)
                {
                    return;
                }
                OnPropertyChanged("PathDir");
            }
        }

        public int SelectedClientIndex
        {
            get
            {
                return _selectedClientIndex;
            }

            set
            {
                if (_selectedClientIndex == value)
                {
                    return;
                }

                // At this point _selectedIndex is the old selected item index

                _selectedClientIndex = value;

                // At this point _selectedIndex is the new selected item index

                OnPropertyChanged("SelectedClientIndex");
            }
        }


        public string Host
        {
            get
            {
                return _host;
            }
            set
            {

                _host = value;
                OnPropertyChanged("Host");
            }
        }

       


        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                if (_currentProgress == value)
                {
                    return;
                }
                _currentProgress = value;
                OnPropertyChanged("CurrentProgress");
            }
        }
        public string User
        {
            get
            {
                return _user;
            }
            set
            {

                _user = value;
                OnPropertyChanged("User");
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {

                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {

                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public string UploadFile
        {
            get
            {
                return _uploadFile;
            }
            set
            {
                _uploadFile = value;
                OnPropertyChanged("UploadFile");
            }
        }

        public ObservableCollection<string> ServerDir
        {
            get
            {
                return _serverDir;
            }
            set
            {
                if (_serverDir == value)
                {
                    return;
                }
                _serverDir = value;
                OnPropertyChanged("ServerDir");
            }
        }

        public ObservableCollection<string> ClientDir
        {
            get
            {
                return _clientDir;
            }
            set
            {
                if (_clientDir == value)
                {
                    return;
                }
                _clientDir = value;
                OnPropertyChanged("ClientDir");
            }
        }



        public DelegateCommand connectCommand;
        public DelegateCommand uploadFileCommand;
        public DelegateCommand downloadFileCommand;
        


        //private startWindow startWindow;

        public ICommand UploadFileCommand
        {
            get
            {
                if (uploadFileCommand == null)
                {
                    uploadFileCommand = new DelegateCommand(Upload_File);
                }
                return uploadFileCommand;
            }
        }

       

        public ICommand DownloadFileCommand
        {
            get
            {
                if (downloadFileCommand == null)
                {
                    downloadFileCommand = new DelegateCommand(Download_File);
                }
                return downloadFileCommand;
            }
        }

        public ICommand ConnectCommand
        {

            get
            {
                if (connectCommand == null)
                {
                    connectCommand = new DelegateCommand(Connect);

                }


                return connectCommand;
            }
        }



        private void Connect()
        {
            pathDir = Directory.GetCurrentDirectory();
            Console.WriteLine("SERVERDIR " + ServerDir.Count);
            try
            {

                client = new FtpClient(_host);
                string[] path = Directory.GetFiles(Directory.GetCurrentDirectory()).Select(Path.GetFileName).ToArray();
                foreach (string s in path)
                {
                    ClientDir.Add(s);
                }

                // specify the login credentials, unless you want to use the "anonymous" user account
                client.Credentials = new NetworkCredential(_user, _password);
                client.Port = _port;

                // begin connecting to the server
                client.Connect();
                //MessageBox.Show("Вы вошли");


                foreach (FtpListItem item in client.GetListing("/"))
                {

                    if ((item.Type == FtpFileSystemObjectType.Directory) || (item.Type == FtpFileSystemObjectType.File))
                        ServerDir.Add((item.FullName).ToString());



                }

            }
            catch
            {
                HandyControl.Controls.MessageBox.Show("Введены неверные данные");
            }
        }



        private void Upload_File()
        {


            file = ClientDir[_selectedClientIndex];



            Console.WriteLine("file " + file);
            try
            {
                
                new Thread(() =>
                {
                    client.UploadFile(@"" + file, "/" + file, progress: c => CurrentProgress = (int)c.Progress);

                    if (CurrentProgress == 100)
                    {
   
                        CurrentProgress = 0;
                        
                    }
                    App.Current.Dispatcher.Invoke(() => ServerDir.Clear());
                    foreach (FtpListItem item in client.GetListing("/"))
                    {

                        if ((item.Type == FtpFileSystemObjectType.Directory) || (item.Type == FtpFileSystemObjectType.File))
                            App.Current.Dispatcher.Invoke(() => ServerDir.Add((item.FullName).ToString()));



                    }


                }).Start();

            }
            catch
            {
                HandyControl.Controls.MessageBox.Show("Не удалось загрузить файл");
            }
            if(CurrentProgress == 100)
            {
                

                
            }
            


        }


        private void Download_File()
        {

            file = ServerDir[_selectedServerIndex];
            ClientDir.Clear();
            string path =  Directory.GetCurrentDirectory();
            Console.WriteLine("Path "+pathDir);
            Console.WriteLine("file " + file);
            try
            {
                new Thread(() =>
                {
                    client.DownloadFile(@"" + path+ file, file, progress: c => CurrentProgress = (int)c.Progress);
                    if (CurrentProgress == 100)
                    {

                        CurrentProgress = 0;

                    }

                    App.Current.Dispatcher.Invoke(() => ClientDir.Clear());
                    string[] pathd = Directory.GetFiles(Directory.GetCurrentDirectory()).Select(Path.GetFileName).ToArray();
                    foreach (string s in pathd)
                    {
                        App.Current.Dispatcher.Invoke(() => ClientDir.Add(s));
                    }


                }).Start();

            }

            
            catch
            {
                HandyControl.Controls.MessageBox.Show("Не удалось загрузить файл");
            }




        }

    }

}

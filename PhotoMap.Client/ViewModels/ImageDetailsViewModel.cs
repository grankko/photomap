using PhotoMap.Client.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoMap.Client.ViewModels
{
    public class ImageDetailsViewModel : ViewModelBase
    {
        public RelayCommand OpenImageCommand { get; set; }
        public string SelectedImageFileName
        {
            get => selectedImageFileName;
            set
            {
                CanOpenImage = !string.IsNullOrEmpty(value);
                selectedImageFileName = value;
                OnPropertyChanged();
            }
        }


        private bool _canOpenImage;
        private string selectedImageFileName;
        private string photoTaken;

        public bool CanOpenImage
        {
            get => _canOpenImage;
            set
            {
                _canOpenImage = value;
                OnPropertyChanged();
            }
        }

        public string PhotoTaken
        {
            get => photoTaken;
            set
            {
                photoTaken = value;
                OnPropertyChanged();
            }
        }

        public ImageDetailsViewModel()
        {
            OpenImageCommand = new RelayCommand(ExecuteOpenImageCommand, CanOpenImageCommandExecute);
        }

        private void ExecuteOpenImageCommand(object obj)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = SelectedImageFileName;
            explorer.Start();
        }

        private bool CanOpenImageCommandExecute(object arg)
        {
            return CanOpenImage;
        }
    }
}

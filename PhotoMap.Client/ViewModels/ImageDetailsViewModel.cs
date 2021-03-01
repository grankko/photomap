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
        private bool _canOpenImage;
        private string _selectedImageFileName;
        private string _photoTaken;

        public string SelectedImageFileName
        {
            get => _selectedImageFileName;
            set
            {
                CanOpenImage = !string.IsNullOrEmpty(value);
                _selectedImageFileName = value;
                OnPropertyChanged();
            }
        }
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
            get => _photoTaken;
            set
            {
                _photoTaken = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenImageCommand { get; set; }

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

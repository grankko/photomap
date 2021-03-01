using Microsoft.Web.WebView2.Wpf;
using PhotoMap.Analyzer;
using PhotoMap.Client.Commands;
using PhotoMap.Client.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace PhotoMap.Client.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private PhotoAnalyzerService _analyzerService = new PhotoAnalyzerService();
        private bool _isReady;
        private string scanDirectoryLabel;
        private string selectedDirectoryProgressLabel;
        private DateTime? _fromDateFilter;

        private DateTime? _toDateFilter;

        public DateTime? ToDateFilter
        {
            get => _toDateFilter;
            set
            {
                _toDateFilter = value;
                OnPropertyChanged();
                if (value.HasValue)
                    ApplyDateFilter();
            }
        }

        public DateTime? FromDateFilter
        {
            get => _fromDateFilter;
            set
            {
                _fromDateFilter = value;
                OnPropertyChanged();
                if (value.HasValue)
                    ApplyDateFilter();
            }
        }

        public bool IsReady
        {
            get => _isReady;
            set
            {
                _isReady = value;
                OnPropertyChanged();
            }
        }

        public string ScanDirectoryLabel
        {
            get => scanDirectoryLabel;
            set
            {
                scanDirectoryLabel = value;
                OnPropertyChanged();
            }
        }

        public string SelectedDirectoryProgressLabel
        {
            get => selectedDirectoryProgressLabel;
            set
            {
                selectedDirectoryProgressLabel = value;
                OnPropertyChanged();
            }
        }


        public RelayCommand ScanDirectoryCommand { get; set; }
        public RelayCommand OpenDirectoryCommand { get; set; }
        public BingMapService BingMapService { get; set; }
        public ImageDetailsViewModel ImageDetailsVM { get; set; }
        public MainViewModel()
        {
            ImageDetailsVM = new ImageDetailsViewModel();
            ScanDirectoryCommand = new RelayCommand(ScanDirectoryCommandExecute, CanScanDirectoryCommandExecute);
            OpenDirectoryCommand = new RelayCommand(OpenDirectoryCommandExecute, CanOpenDirectoryCommandExecute);

            _analyzerService.Worker.ProgressChanged += Worker_ProgressChanged;
            _analyzerService.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            ScanDirectoryLabel = "Scan directory";
        }

        private bool CanOpenDirectoryCommandExecute(object arg)
        {
            return (!string.IsNullOrEmpty(_analyzerService.DirectoryPath));
        }

        private void OpenDirectoryCommandExecute(object obj)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = _analyzerService.DirectoryPath;
            explorer.Start();
        }

        public void SetBingMapService(WebView2 webView)
        {
            BingMapService = new BingMapService(webView); //todo: fix
            BingMapService.BingMapLoaded += BingMapService_BingMapLoaded;
            BingMapService.PinClicked += BingMapService_PinClicked;
        }

        private bool CanScanDirectoryCommandExecute(object arg)
        {
            return true;
        }

        private void ScanDirectoryCommandExecute(object obj)
        {
            FromDateFilter = null;
            ToDateFilter = null;

            if (_analyzerService.Worker.IsBusy)
            {
                _analyzerService.CancelAnalysis();
            }
            else
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        IsReady = false;
                        BingMapService.ClearPins();

                        var folder = dialog.SelectedPath;
                        _analyzerService.ScanDirectory(folder);
                        _analyzerService.StartAnalysis();
                        ScanDirectoryLabel = "Cancel";
                    }
                }
            }
        }

        private void BingMapService_PinClicked(object sender, PinClickedEventArgs e)
        {
            var clickedImage = _analyzerService.Result.First(r => r.Id == e.Id);
            ImageDetailsVM.SelectedImageFileName = clickedImage.FileName;
            ImageDetailsVM.PhotoTaken = clickedImage.PhotoTaken.HasValue ? clickedImage.PhotoTaken.Value.ToShortDateString() : "-";
        }

        private void BingMapService_BingMapLoaded(object sender, EventArgs e)
        {
            IsReady = true;
        }

        private void Worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            ScanDirectoryLabel = "Scan directory";

            if (_analyzerService.Result.Count > 0)
                IsReady = true;
        }

        private void Worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            SelectedDirectoryProgressLabel = $"{_analyzerService.DirectoryPath} ({_analyzerService.ImageFilesProcessed}/{_analyzerService.ImageFileCount})";
            PhotoMetadataModel image = (PhotoMetadataModel)e.UserState;

            if (image.HasGpsData)
                BingMapService.SetPushpin(image.FormatedLatitude, image.FormatedLongitude, image.Id);
        }

        private void ApplyDateFilter()
        {
            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MaxValue;

            if (FromDateFilter.HasValue)
                from = FromDateFilter.Value;
            if (ToDateFilter.HasValue)
                to = ToDateFilter.Value;

            foreach (var result in _analyzerService.Result)
            {
                var shouldShow = result.PhotoTaken.HasValue && result.PhotoTaken.Value >= from && result.PhotoTaken.Value <= to;
                BingMapService.TogglePinVisibility(result.Id, shouldShow);
            }
        }
    }
}





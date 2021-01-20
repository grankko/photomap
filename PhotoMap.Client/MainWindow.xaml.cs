using PhotoMap.Analyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoMap.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <remarks>todo: introcude ViewModel. Implement date filtering.</remarks>
    public partial class MainWindow : Window
    {
        private PhotoAnalyzerService _analyzerService = new PhotoAnalyzerService();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var html = File.ReadAllText("index.html");
            await webView.EnsureCoreWebView2Async();
            webView.NavigateToString(html);

            _analyzerService.Worker.ProgressChanged += Worker_ProgressChanged;
            _analyzerService.Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            ScanDirectory.Content = "Scan directory";

            if (_analyzerService.Result.Count > 0)
            {
                FromFilter.IsEnabled = true;
                ToFilter.IsEnabled = true;
            }
        }

        private void Worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            DirectoryInfoLinkText.Text = $"{_analyzerService.DirectoryPath} ({_analyzerService.ImageFilesProcessed}/{_analyzerService.ImageFileCount})";
            PhotoMetadataModel image = (PhotoMetadataModel)e.UserState;

            if (image.HasGpsData)
                SetPushpin(image.FormatedLatitude, image.FormatedLongitude, image.Id);
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonSerializer.Deserialize<WebViewEventMessage>(e.WebMessageAsJson, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            if (message.Event == "BingMapLoaded")
            {
                ScanDirectory.IsEnabled = true;
            }
            else if (message.Event == "PinClicked")
            {
                var id = Guid.Parse(message.Parameter);
                var clickedImage = _analyzerService.Result.First(r => r.Id == id);

                PreviewImage.Source = new BitmapImage(new Uri(clickedImage.FileName));
                PreviewImage.Tag = clickedImage.FileName;
                PreviewImage.Visibility = Visibility.Visible;

                PhotoTakenLabel.Visibility = Visibility.Visible;
                PhotoTakenValue.Visibility = Visibility.Visible;
                PhotoTakenValue.Content = clickedImage.PhotoTaken.HasValue ? clickedImage.PhotoTaken.Value.ToShortDateString() : "-";

            }
        }
        private async void SetPushpin(string latitude, string longitude, Guid id)
        {
            var setPinScript = $"setPin('{ latitude}', '{ longitude}', '{ id }');";
            await webView.ExecuteScriptAsync(setPinScript);
        }

        private void ScanDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (_analyzerService.Worker.IsBusy)
            {
                _analyzerService.CancelAnalysis();
            } else
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        PhotoTakenLabel.Visibility = Visibility.Hidden;
                        PhotoTakenValue.Visibility = Visibility.Hidden;
                        PreviewImage.Visibility = Visibility.Hidden;
                        FromFilter.SelectedDate = null;
                        FromFilter.IsEnabled = false;
                        ToFilter.SelectedDate = null;
                        ToFilter.IsEnabled = false;

                        webView.ExecuteScriptAsync("clearPins();");

                        var folder = dialog.SelectedPath;
                        _analyzerService.ScanDirectory(folder);
                        DirectoryInfoLink.Tag = folder;
                        _analyzerService.StartAnalysis();
                        ScanDirectory.Content = "Cancel";
                    }
                }
            }
        }

        private void DirectoryInfoLink_Click(object sender, RoutedEventArgs e)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = ((Hyperlink)sender).Tag.ToString();
            explorer.Start();
        }

        private void PreviewImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process explorer = new Process();
            explorer.StartInfo.FileName = "explorer.exe";
            explorer.StartInfo.Arguments = ((Image)sender).Tag.ToString();
            explorer.Start();
        }

        private async void Filter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MaxValue;

            if (FromFilter.SelectedDate.HasValue)
                from = FromFilter.SelectedDate.Value;
            if (ToFilter.SelectedDate.HasValue)
                to = ToFilter.SelectedDate.Value;

            foreach (var result in _analyzerService.Result)
            {
                var shouldShow = result.PhotoTaken.HasValue && result.PhotoTaken.Value >= from && result.PhotoTaken.Value <= to;

                var togglePinVisibilityScript = $"togglePinVisibility('{ result.Id }', {shouldShow.ToString().ToLower()});";
                await webView.ExecuteScriptAsync(togglePinVisibilityScript);
            }
        }
    }
}

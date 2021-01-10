using PhotoMap.Analyzer;
using System;
using System.Collections.Generic;
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
        }

        private void Worker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            DirectoryInfoLabel.Content = $"{_analyzerService.DirectoryPath} ({_analyzerService.ImageFilesProcessed}/{_analyzerService.ImageFileCount})";
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
            PhotoTakenLabel.Visibility = Visibility.Hidden;
            PhotoTakenValue.Visibility = Visibility.Hidden;

            webView.ExecuteScriptAsync("clearPins();");

            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    _analyzerService.ScanDirectory(folder);
                    DirectoryInfoLabel.Content = $"{folder} ({_analyzerService.ImageFilesProcessed}/{_analyzerService.ImageFileCount})";

                    _analyzerService.StartAnalysis();
                }
            }
        }
    }
}

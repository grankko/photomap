using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var html = File.ReadAllText("index.html");
            await webView.EnsureCoreWebView2Async();
            webView.NavigateToString(html);
        }

        private void webView_WebMessageReceived(object sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonSerializer.Deserialize<WebViewEventMessage>(e.WebMessageAsJson, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            if (message.Event == "BingMapLoaded")
            {
                // ready to interact
            }
            else if (message.Event == "PinClicked")
            {
                var imagePath = System.Web.HttpUtility.UrlDecode(message.Parameter);
                PreviewImage.Source = new BitmapImage(new Uri(imagePath));
            }
        }
        private async void SetPushpin(string latitude, string longitude, string filePath)
        {
            var setPinScript = $@"
                var pin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location({latitude}, {longitude}));
                pin.filePath = escape('{System.Web.HttpUtility.JavaScriptStringEncode(filePath)}');
                this.map.entities.push(pin);    
                Microsoft.Maps.Events.addHandler(pin, 'click', pinClick);
                ";
            await webView.ExecuteScriptAsync(setPinScript);
        }

        private void ScanDirectory_Click(object sender, RoutedEventArgs e)
        {

            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    var imageFiles = System.IO.Directory.GetFiles(folder, "*.jpg", SearchOption.AllDirectories);
                    foreach (var imageFile in imageFiles)
                    {
                        try
                        {
                            var gps = ImageMetadataReader.ReadMetadata(imageFile)
                                             .OfType<GpsDirectory>()
                                             .FirstOrDefault();

                            if (gps != null)
                            {
                                var location = gps.GetGeoLocation();
                                if (location != null)
                                {
                                    var numberFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };
                                    SetPushpin(location.Latitude.ToString(numberFormat), location.Longitude.ToString(numberFormat), imageFile);
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }
            }
        }
    }
}

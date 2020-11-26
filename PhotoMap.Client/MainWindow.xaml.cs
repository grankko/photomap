using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
            //SetPushpin("59.452644095886846", "17.933108726842157");

            var gps = ImageMetadataReader.ReadMetadata("pike.jpg")
                             .OfType<GpsDirectory>()
                             .FirstOrDefault();

            var location = gps.GetGeoLocation();
            var numberFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };
            SetPushpin(location.Latitude.ToString(numberFormat), location.Longitude.ToString(numberFormat));
        }

        private async void SetPushpin(string latitude, string longitude)
        {
            var setPinScript = $@"
                var pin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location({latitude}, {longitude}));
                this.map.entities.push(pin);
                ";
            await webView.ExecuteScriptAsync(setPinScript);
        }
    }
}

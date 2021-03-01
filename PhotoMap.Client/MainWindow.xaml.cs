using PhotoMap.Analyzer;
using PhotoMap.Client.Services;
using PhotoMap.Client.ViewModels;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((MainViewModel)DataContext).SetBingMapService(webView); // todo:fix
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Loads html view of BingMaps to the wpf control
            var html = File.ReadAllText("index.html");
            await webView.EnsureCoreWebView2Async();
            webView.NavigateToString(html);
        }
    }
}

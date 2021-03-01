using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhotoMap.Client.Services
{
    public class BingMapService
    {
        public event EventHandler BingMapLoaded;
        public event EventHandler<PinClickedEventArgs> PinClicked;

        private WebView2 _webView;

        public BingMapService(WebView2 webView)
        {
            _webView = webView;
            _webView.WebMessageReceived += _webView_WebMessageReceived;
        }

        public async void SetPushpin(string latitude, string longitude, Guid id)
        {
            var setPinScript = $"setPin('{ latitude}', '{ longitude}', '{ id }');";
            await _webView.ExecuteScriptAsync(setPinScript);
        }

        public async void ClearPins()
        {
            await _webView.ExecuteScriptAsync("clearPins();");
        }

        public async void TogglePinVisibility(Guid id, bool shouldShow)
        {
            var togglePinVisibilityScript = $"togglePinVisibility('{ id }', {shouldShow.ToString().ToLower()});";
            await _webView.ExecuteScriptAsync(togglePinVisibilityScript);
        }

        private void _webView_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = JsonSerializer.Deserialize<WebViewEventMessage>(e.WebMessageAsJson, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

            if (message.Event == "BingMapLoaded")
            {
                BingMapLoaded?.Invoke(this, new EventArgs());
            }
            else if (message.Event == "PinClicked")
            {
                var id = Guid.Parse(message.Parameter);
                PinClicked?.Invoke(this, new PinClickedEventArgs(id));
            }
        }
    }


    public class PinClickedEventArgs : EventArgs
    {
        public Guid Id { get; set; }
        public PinClickedEventArgs(Guid id)
        {
            Id = id;
        }
    }
}

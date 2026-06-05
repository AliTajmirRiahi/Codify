using Codify.Infrastructure;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Codify.UI.ToolWindows
{
    /// <summary>
    /// Interaction logic for CodifyToolWindowControl.
    /// </summary>
    public partial class CodifyToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodifyToolWindowControl"/> class.
        /// </summary>
        public CodifyToolWindowControl()
        {
            this.InitializeComponent();
            _ = InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                // 1. Define the user data folder path to avoid permission issues.
                var userDataFolder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CodifyExtension");

                // 2. Create the WebView2 environment using the custom folder.
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                // 3. Initialize WebView2 with the environment.
                await WebView.EnsureCoreWebView2Async(environment);

                WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                // 4. Load initial HTML content for testing.
                var html = GetEmbeddedResource(ResourceConfig.ChatHtml);
                var css = GetEmbeddedResource(ResourceConfig.ChatCss);
                var js = GetEmbeddedResource(ResourceConfig.ChatJs);

                // 2. Inject CSS & JS into HTML

                var finalHtml = html
                    .Replace("<!-- CSS_PLACEHOLDER -->", $"<style>{css}</style>")
                    .Replace("<!-- JS_PLACEHOLDER -->", $"<script>{js}</script>");

                WebView.NavigateToString(!string.IsNullOrEmpty(finalHtml)
                    ? finalHtml
                    : "<h1>Error: UI Resources not found!</h1>");

                WebView.NavigationCompleted += (s, e) => UpdateTheme();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging.
                System.Diagnostics.Debug.WriteLine($"WebView2 Error: {ex.Message}");
                MessageBox.Show($"WebView2 Init Failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var json = e.WebMessageAsJson;

            Task.Delay(800).ContinueWith(_ =>
            {
                var response ="{\"type\":\"AI_RESPONSE\",\"payload\":\"Message received ✅\"}";

                WebView.Dispatcher.Invoke(() =>
                {
                    WebView.CoreWebView2.PostWebMessageAsJson(response);
                });
            });
        }
        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            UpdateTheme();
        }

        private void UpdateTheme()
        {
            var bg = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            var fg = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey);
            var border = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBorderColorKey);
            var button = VSColorTheme.GetThemedColor(EnvironmentColors.SystemHighlightColorKey);

            string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

            string script = $@"
                document.documentElement.style.setProperty('--vs-background', '{ToHex(bg)}');
                document.documentElement.style.setProperty('--vs-foreground', '{ToHex(fg)}');
                document.documentElement.style.setProperty('--vs-border', '{ToHex(border)}');
                document.documentElement.style.setProperty('--vs-input-bg', '{ToHex(border)}');
                document.documentElement.style.setProperty('--vs-button-bg', '{ToHex(button)}');
                document.documentElement.style.setProperty('--vs-button-hover', '{ToHex(button)}');
            ";

            WebView.CoreWebView2?.ExecuteScriptAsync(script);
        }
        private string GetEmbeddedResource(string resourcePath)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Resource not found: {resourcePath}");
                    return string.Empty;
                }
                using (var reader = new System.IO.StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
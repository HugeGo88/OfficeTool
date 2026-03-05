using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace OfficeTool.Services;

public class PdfExportService : Contracts.Services.IPdfExportService
{
    private WebView2? _webView;

    public void RegisterWebView(WebView2 webView)
    {
        _webView = webView;
    }

    public async Task<bool> ExportToPdfAsync(string htmlContent, string suggestedFileName)
    {
        if (_webView?.CoreWebView2 == null)
        {
            return false;
        }

        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("PDF Document", new List<string> { ".pdf" });
        picker.SuggestedFileName = suggestedFileName.Replace(".md", "").Replace(".txt", "");

        var file = await picker.PickSaveFileAsync();
        if (file == null)
        {
            return false;
        }

        try
        {
            var printSettings = _webView.CoreWebView2.Environment.CreatePrintSettings();
            printSettings.ShouldPrintBackgrounds = true;
            printSettings.ShouldPrintHeaderAndFooter = false;

            var result = await _webView.CoreWebView2.PrintToPdfAsync(file.Path, printSettings);
            return result;
        }
        catch
        {
            return false;
        }
    }
}

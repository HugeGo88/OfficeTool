namespace OfficeTool.Contracts.Services;

public interface IPdfExportService
{
    Task<bool> ExportToPdfAsync(string htmlContent, string suggestedFileName);
    void RegisterWebView(Microsoft.UI.Xaml.Controls.WebView2 webView);
}

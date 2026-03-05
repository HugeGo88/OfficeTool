using Microsoft.UI.Xaml.Controls;
using OfficeTool.Contracts.Services;
using OfficeTool.ViewModels;

namespace OfficeTool.Views;

public sealed partial class WriterPage : Page
{
    public WriterViewModel ViewModel { get; }

    public WriterPage()
    {
        ViewModel = App.GetService<WriterViewModel>();
        InitializeComponent();

        // Register the WebView2 with the PDF export service after it's loaded
        PreviewWebView.Loaded += (s, e) =>
        {
            var pdfExportService = App.GetService<IPdfExportService>();
            pdfExportService.RegisterWebView(PreviewWebView);
        };
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace OfficeTool.Helpers;

public static class WebView2Helper
{
    public static readonly DependencyProperty HtmlContentProperty =
        DependencyProperty.RegisterAttached(
            "HtmlContent",
            typeof(string),
            typeof(WebView2Helper),
            new PropertyMetadata(null, OnHtmlContentChanged));

    public static string GetHtmlContent(DependencyObject obj)
    {
        return (string)obj.GetValue(HtmlContentProperty);
    }

    public static void SetHtmlContent(DependencyObject obj, string value)
    {
        obj.SetValue(HtmlContentProperty, value);
    }

    private static async void OnHtmlContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is WebView2 webView && e.NewValue is string html)
        {
            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }

            webView.NavigateToString(html);
        }
    }
}

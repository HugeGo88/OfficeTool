using System;
using System.IO;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using OfficeTool.Attributes;
using OfficeTool.Contracts.Services;
using OfficeTool.Models;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace OfficeTool.ViewModels;

[NavigationItem("Writer", "\uE70F", "Shell_Writer", 0)]
public partial class WriterViewModel : ObservableRecipient
{
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly IPdfExportService _pdfExportService;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _company = string.Empty;

    [ObservableProperty]
    private DateTimeOffset _date = DateTimeOffset.Now;

    [ObservableProperty]
    private bool _customDate = false;

    [ObservableProperty]
    private bool _header = false;

    [ObservableProperty]
    private bool _signing = false;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string _markdownText = string.Empty;

    [ObservableProperty]
    private string _htmlContent = string.Empty;

    [ObservableProperty]
    private Letter _currentLetter = new();

    public string FormattedDate => Date.ToString("dd.MM.yyyy");

    public WriterViewModel(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    partial void OnMarkdownTextChanged(string value)
    {
        UpdateHtmlContent();
    }

    partial void OnNameChanged(string value) => UpdateHtmlContent();
    partial void OnLastNameChanged(string value) => UpdateHtmlContent();
    partial void OnAddressChanged(string value) => UpdateHtmlContent();
    partial void OnCityChanged(string value) => UpdateHtmlContent();
    partial void OnCompanyChanged(string value) => UpdateHtmlContent();
    partial void OnDateChanged(DateTimeOffset value)
    {
        OnPropertyChanged(nameof(FormattedDate));
        UpdateHtmlContent();
    }
    partial void OnSubjectChanged(string value) => UpdateHtmlContent();

    private void UpdateHtmlContent()
    {
        var processedMarkdown = ReplacePlaceholders(MarkdownText);
        HtmlContent = ConvertMarkdownToHtml(processedMarkdown);
    }

    private string ReplacePlaceholders(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return markdown;

        return markdown
            .Replace("{{NAME}}", Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{{LASTNAME}}", LastName, StringComparison.OrdinalIgnoreCase)
            .Replace("{{ADDRESS}}", Address, StringComparison.OrdinalIgnoreCase)
            .Replace("{{CITY}}", City, StringComparison.OrdinalIgnoreCase)
            .Replace("{{COMPANY}}", Company, StringComparison.OrdinalIgnoreCase)
            .Replace("{{DATE}}", Date.ToString("dd.MM.yyyy"), StringComparison.OrdinalIgnoreCase)
            .Replace("{{SUBJECT}}", Subject, StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private async Task SaveToPdfAsync()
    {
        var fileName = string.IsNullOrWhiteSpace(MarkdownText) ? "document" : "document";
        await _pdfExportService.ExportToPdfAsync(HtmlContent, fileName);
    }

    [RelayCommand]
    private async Task SaveLetterAsync()
    {
        SyncViewModelToLetter();

        var picker = new FileSavePicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("XML Document", new List<string> { ".xml" });
        picker.SuggestedFileName = string.IsNullOrWhiteSpace(Subject) ? "letter" : Subject;

        var file = await picker.PickSaveFileAsync();
        if (file == null)
        {
            return;
        }

        try
        {
            _currentLetter.Path = file.Path;

            var serializer = new XmlSerializer(typeof(Letter));
            using var stream = new FileStream(file.Path, FileMode.Create);
            serializer.Serialize(stream, _currentLetter);
        }
        catch
        {
        }
    }

    [RelayCommand]
    private async Task LoadLetterAsync()
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".xml");

        var file = await picker.PickSingleFileAsync();
        if (file == null)
        {
            return;
        }

        try
        {
            var serializer = new XmlSerializer(typeof(Letter));
            using var stream = new FileStream(file.Path, FileMode.Open);
            var loadedLetter = (Letter)serializer.Deserialize(stream);

            if (loadedLetter != null)
            {
                _currentLetter = loadedLetter;
                SyncLetterToViewModel();
            }
        }
        catch
        {
        }
    }

    private void SyncViewModelToLetter()
    {
        _currentLetter.Header = Header;
        _currentLetter.Company = Company;
        _currentLetter.FirstName = Name;
        _currentLetter.LastName = LastName;
        _currentLetter.Street = Address;
        _currentLetter.City = City;
        _currentLetter.Signing = Signing;
        _currentLetter.SetDate = Date;
        _currentLetter.CustomeDate = CustomDate;
        _currentLetter.Subject = Subject;
        _currentLetter.Content = MarkdownText;
    }

    private void SyncLetterToViewModel()
    {
        Header = _currentLetter.Header;
        Company = _currentLetter.Company;
        Name = _currentLetter.FirstName;
        LastName = _currentLetter.LastName;
        Address = _currentLetter.Street;
        City = _currentLetter.City;
        Signing = _currentLetter.Signing;
        Date = _currentLetter.SetDate;
        CustomDate = _currentLetter.CustomeDate;
        Subject = _currentLetter.Subject;
        MarkdownText = _currentLetter.Content;
    }

    private string ConvertMarkdownToHtml(string markdown)
    {
        var htmlContent = Markdown.ToHtml(markdown ?? string.Empty, _markdownPipeline);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;            background-color: #f0f0f0;            padding: 20px;            margin: 0;        }}        .page {{            max-width: 210mm;            min-height: 297mm;            margin: 0 auto 20px auto;            background-color: white;            padding: 25mm;            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);            box-sizing: border-box;            line-height: 1.6;            color: #333;        }}
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
            page-break-after: avoid;
            page-break-inside: avoid;
        }}
        h1 {{
            font-size: 2em;
            border-bottom: 1px solid #eaecef;
            padding-bottom: 0.3em;
        }}
        h2 {{
            font-size: 1.5em;
            border-bottom: 1px solid #eaecef;
            padding-bottom: 0.3em;
        }}
        code {{
            background-color: rgba(27, 31, 35, 0.05);
            border-radius: 3px;
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 85%;
            padding: 0.2em 0.4em;
        }}
        pre {{
            background-color: #f6f8fa;
            border-radius: 3px;
            padding: 16px;
            overflow: auto;
        }}
        pre code {{
            background-color: transparent;
            padding: 0;
        }}
        blockquote {{
            border-left: 4px solid #dfe2e5;
            color: #6a737d;
            padding: 0 1em;
            margin: 0;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
        }}
        table th, table td {{
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
        }}
        table tr:nth-child(2n) {{
            background-color: #f6f8fa;
        }}
        a {{
            color: #0366d6;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        img {{
            max-width: 100%;
        }}

        @page {{
            size: A4;
            margin: 0%;
        }}

        @media print {{
            body {{            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;            background-color: #f0f0f0;            padding: 20px;            margin: 0;        }}        .page {{            max-width: 210mm;            min-height: 297mm;            margin: 0 auto 20px auto;            background-color: white;            padding: 25mm;            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);            box-sizing: border-box;            line-height: 1.6;            color: #333;        }}
            table th, table td {{
                border-color: #000;
            }}
            table tr:nth-child(2n) {{
                background-color: #f6f8fa;
            }}
            a {{
                color: #0366d6;
            }}
        }}

        @media (prefers-color-scheme: dark) {{            body {{                background-color: #0d0d0d;            }}            .page {{                background-color: #1e1e1e;                color: #d4d4d4;                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);            }}
            h1, h2 {{
                border-bottom-color: #444;
            }}
            code {{
                background-color: rgba(255, 255, 255, 0.1);
            }}
            pre {{
                background-color: #2d2d2d;
            }}
            blockquote {{
                border-left-color: #444;
                color: #999;
            }}
            table th, table td {{
                border-color: #444;
            }}
            table tr:nth-child(2n) {{
                background-color: #2d2d2d;
            }}
            a {{
                color: #58a6ff;
            }}
        }}
    </style>
</head>
<body>
<div class='page'>
    {htmlContent}
</div>
</body>
</html>";
    }
}

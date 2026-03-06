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
    private readonly ICssStyleService _cssStyleService;

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

    public WriterViewModel(IPdfExportService pdfExportService, ICssStyleService cssStyleService)
    {
        _pdfExportService = pdfExportService;
        _cssStyleService = cssStyleService;
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
        return _cssStyleService.WrapInHtmlDocument(htmlContent);
    }
}
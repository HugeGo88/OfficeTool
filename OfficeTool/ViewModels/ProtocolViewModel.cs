using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using OfficeTool.Attributes;
using OfficeTool.Contracts.Services;
using OfficeTool.Models;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OfficeTool.ViewModels;

[NavigationItem("Protocol", "\uE7C3", "Shell_Protocol", 1)]
public partial class ProtocolViewModel : ObservableRecipient
{
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly IPdfExportService _pdfExportService;
    private readonly ICssStyleService _cssStyleService;

    [ObservableProperty]
    private Protocol _currentProtocol;

    public ProtocolViewModel(IPdfExportService pdfExportService, ICssStyleService cssStyleService)
    {
        _pdfExportService = pdfExportService;
        _cssStyleService = cssStyleService;
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
        _currentProtocol = new Protocol();
        AddTopic();
    }

    [RelayCommand]
    private void AddTopic()
    {
        var newTopic = new ProtocolTopic
        {
            Number = CurrentProtocol.Topics.Count + 1
        };
        CurrentProtocol.Topics.Add(newTopic);
    }

    [RelayCommand]
    private void RemoveTopic(ProtocolTopic topic)
    {
        if (CurrentProtocol.Topics.Contains(topic))
        {
            CurrentProtocol.Topics.Remove(topic);
            RenumberTopics();
        }
    }

    [RelayCommand]
    private void AddActionPoint(ProtocolTopic topic)
    {
        if (topic != null)
        {
            var newActionPoint = new ActionPoint();
            topic.ActionPoints.Add(newActionPoint);
        }
    }

    [RelayCommand]
    private void RemoveActionPoint(object[] parameters)
    {
        if (parameters != null && parameters.Length == 2
            && parameters[0] is ProtocolTopic topic
            && parameters[1] is ActionPoint actionPoint)
        {
            if (topic.ActionPoints.Contains(actionPoint))
            {
                topic.ActionPoints.Remove(actionPoint);
            }
        }
    }

    private void RenumberTopics()
    {
        var topicNumber = 1;
        foreach (var topic in CurrentProtocol.Topics)
        {
            topic.Number = topicNumber++;
        }
    }

    [RelayCommand]
    private void NewProtocol()
    {
        CurrentProtocol = new Protocol();
        AddTopic();
    }

    [RelayCommand]
    private async Task ExportToPdf()
    {
        var html = GenerateProtocolHtml();
        var fileName = string.IsNullOrWhiteSpace(CurrentProtocol.Subject)
            ? $"Protocol_{CurrentProtocol.MeetingDate:yyyy-MM-dd}"
            : CurrentProtocol.Subject;
        await _pdfExportService.ExportToPdfAsync(html, fileName);
    }

    [RelayCommand]
    private async Task ExportToMarkdown()
    {
        var markdown = GenerateMarkdown();

        var picker = new FileSavePicker();
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("Markdown Document", new List<string> { ".md" });
        picker.SuggestedFileName = string.IsNullOrWhiteSpace(CurrentProtocol.Subject)
            ? $"Protocol_{CurrentProtocol.MeetingDate:yyyy-MM-dd}"
            : CurrentProtocol.Subject;

        var file = await picker.PickSaveFileAsync();
        if (file == null)
        {
            return;
        }

        try
        {
            await Windows.Storage.FileIO.WriteTextAsync(file, markdown);
        }
        catch
        {
            // Handle save error
        }
    }

    private string GenerateProtocolHtml()
    {
        var markdown = GenerateMarkdown();
        var htmlContent = Markdown.ToHtml(markdown, _markdownPipeline);
        return _cssStyleService.WrapInHtmlDocument(htmlContent);
    }

    private string GenerateMarkdown()
    {
        StringBuilder sb = new StringBuilder();

        // Header similar to the Excel template
        string subject = !string.IsNullOrWhiteSpace(CurrentProtocol.Subject)
            ? CurrentProtocol.Subject
            : "-";

        sb.AppendLine($"# {subject} - {CurrentProtocol.MeetingDate:dd.MM.yyyy}");
        sb.AppendLine();

        // Personenblock angelehnt an die Vorlage
        string attendees = !string.IsNullOrWhiteSpace(CurrentProtocol.Attendees)
            ? CurrentProtocol.Attendees
            : "-";
        string absentees = !string.IsNullOrWhiteSpace(CurrentProtocol.Absentees)
            ? CurrentProtocol.Absentees
            : "-";
        string excused = !string.IsNullOrWhiteSpace(CurrentProtocol.Excused)
            ? CurrentProtocol.Excused
            : "-";
        string meetingLead = !string.IsNullOrWhiteSpace(CurrentProtocol.MeetingLead)
            ? CurrentProtocol.MeetingLead
            : "-";
        string recorder = !string.IsNullOrWhiteSpace(CurrentProtocol.Recorder)
            ? CurrentProtocol.Recorder
            : "-";

        sb.AppendLine($"**Teilnehmer:** {attendees}");
        sb.AppendLine();
        sb.AppendLine($"**Abwesend:** {absentees}");
        sb.AppendLine();
        sb.AppendLine($"**Entschuldigt:** {excused}");
        sb.AppendLine();
        sb.AppendLine($"**Sitzungsleitung:** {meetingLead} **Protokolant:in:** {recorder}");
        sb.AppendLine();


        // Themen- und Aufgaben-Tabelle angelehnt an die Excel-Struktur
        if (CurrentProtocol.Topics.Count > 0)
        {
            sb.AppendLine("| # | Thema |");
            sb.AppendLine("|---|-------|");

            foreach (ProtocolTopic topic in CurrentProtocol.Topics)
            {
                string topicTitle = !string.IsNullOrWhiteSpace(topic.Title)
                    ? topic.Title
                    : "-";

                string contentMarkdown = !string.IsNullOrWhiteSpace(topic.Content)
                    ? topic.Content
                    : string.Empty;

                string contentHtml = string.Empty;
                if (!string.IsNullOrEmpty(contentMarkdown))
                {
                    // Convert markdown content to HTML so it can be embedded inside the table cell
                    contentHtml = Markdown.ToHtml(contentMarkdown.TrimEnd(), _markdownPipeline)
;
                }

                sb.AppendLine($"| **{topic.Number}** | **{topicTitle}** |");

                if (!string.IsNullOrEmpty(contentHtml))
                {
                    sb.AppendLine($"|   | {contentHtml} |");
                }

                foreach (ActionPoint actionPoint in topic.ActionPoints)
                {
                    string taskText = actionPoint.Task;
                    string personText = !string.IsNullOrWhiteSpace(actionPoint.Person)
                        ? actionPoint.Person
                        : string.Empty;
                    string dueDateText = actionPoint.DueDate.ToString("dd.MM.yyyy");
                    string statusText = actionPoint.IsCompleted ? "[x]" : "[ ]";

                    sb.AppendLine($"| | {statusText} {taskText} {personText} {dueDateText}|");
                }
            }
        }

        return sb.ToString();
    }
}

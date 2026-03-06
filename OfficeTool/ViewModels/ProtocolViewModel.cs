using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Markdig;
using OfficeTool.Attributes;
using OfficeTool.Contracts.Services;
using OfficeTool.Models;
using System.Linq;
using System.Text;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OfficeTool.ViewModels;

[NavigationItem("Protocol", "\uE7C3", "Shell_Protocol", 1)]
public partial class ProtocolViewModel : ObservableRecipient
{
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly IPdfExportService _pdfExportService;

    [ObservableProperty]
    private Protocol _currentProtocol;

    public ProtocolViewModel(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
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

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background-color: #f0f0f0;
            padding: 20px;
            margin: 0;
        }}
        .page {{
            max-width: 210mm;
            min-height: 297mm;
            margin: 0 auto 20px auto;
            background-color: white;
            padding: 25mm;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            box-sizing: border-box;
            line-height: 1.6;
            color: #333;
        }}
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
            page-break-after: avoid;
            page-break-inside: avoid;
        }}
        h1 {{ font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h2 {{ font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h3 {{ font-size: 1.25em; }}
        h4 {{ font-size: 1em; }}
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
        pre code {{ background-color: transparent; padding: 0; }}
        blockquote {{
            border-left: 4px solid #dfe2e5;
            color: #6a737d;
            padding: 0 1em;
            margin: 0;
        }}
        table {{ border-collapse: collapse; width: 100%; }}
        table th, table td {{ border: 1px solid #dfe2e5; padding: 6px 13px; }}
        table tr:nth-child(2n) {{ background-color: #f6f8fa; }}
        ul {{ list-style-type: none; padding-left: 0; }}
        ul li {{ padding: 2px 0; }}
        a {{ color: #0366d6; text-decoration: none; }}
        a:hover {{ text-decoration: underline; }}
        img {{ max-width: 100%; }}
        @page {{ size: A4; margin: 0%; }}
        @media print {{
            body {{ background-color: #f0f0f0; padding: 20px; margin: 0; }}
            .page {{
                max-width: 210mm;
                min-height: 297mm;
                margin: 0 auto 20px auto;
                background-color: white;
                padding: 25mm;
                box-shadow: none;
                box-sizing: border-box;
                line-height: 1.6;
                color: #333;
            }}
            table th, table td {{ border-color: #000; }}
        }}
        @media (prefers-color-scheme: dark) {{
            body {{ background-color: #0d0d0d; }}
            .page {{ background-color: #1e1e1e; color: #d4d4d4; box-shadow: 0 2px 8px rgba(0,0,0,0.3); }}
            h1, h2 {{ border-bottom-color: #444; }}
            code {{ background-color: rgba(255,255,255,0.1); }}
            pre {{ background-color: #2d2d2d; }}
            blockquote {{ border-left-color: #444; color: #999; }}
            table th, table td {{ border-color: #444; }}
            table tr:nth-child(2n) {{ background-color: #2d2d2d; }}
            a {{ color: #58a6ff; }}
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

    private string GenerateMarkdown()
    {
        var sb = new StringBuilder();

        // Header section
        if (!string.IsNullOrWhiteSpace(CurrentProtocol.Subject))
        {
            sb.AppendLine($"# {CurrentProtocol.Subject}");
            sb.AppendLine();
        }

        // Meeting date
        sb.AppendLine($"**Meeting Date:** {CurrentProtocol.MeetingDate:dddd, MMMM d, yyyy}");
        sb.AppendLine();

        // Attendees
        if (!string.IsNullOrWhiteSpace(CurrentProtocol.Attendees))
        {
            sb.AppendLine("## Attendees");
            sb.AppendLine(CurrentProtocol.Attendees);
            sb.AppendLine();
        }

        // Absentees
        if (!string.IsNullOrWhiteSpace(CurrentProtocol.Absentees))
        {
            sb.AppendLine("## Absentees");
            sb.AppendLine(CurrentProtocol.Absentees);
            sb.AppendLine();
        }

        // Excused
        if (!string.IsNullOrWhiteSpace(CurrentProtocol.Excused))
        {
            sb.AppendLine("## Excused");
            sb.AppendLine(CurrentProtocol.Excused);
            sb.AppendLine();
        }

        // Topics and action points
        if (CurrentProtocol.Topics.Count > 0)
        {
            sb.AppendLine("## Meeting Topics");
            sb.AppendLine();

            foreach (var topic in CurrentProtocol.Topics)
            {
                sb.AppendLine($"### Topic {topic.Number}");

                if (!string.IsNullOrWhiteSpace(topic.Title))
                {
                    sb.AppendLine($"**Title:** {topic.Title}");
                    sb.AppendLine();
                }

                if (!string.IsNullOrWhiteSpace(topic.Content))
                {
                    sb.AppendLine(topic.Content);
                    sb.AppendLine();
                }

                // Action points for this topic
                if (topic.ActionPoints.Count > 0)
                {
                    sb.AppendLine("#### Action Points");
                    sb.AppendLine();

                    foreach (var actionPoint in topic.ActionPoints)
                    {
                        var taskText = !string.IsNullOrWhiteSpace(actionPoint.Task) 
                            ? actionPoint.Task 
                            : "(No description)";
                        var personText = !string.IsNullOrWhiteSpace(actionPoint.Person) 
                            ? $" - {actionPoint.Person}" 
                            : string.Empty;
                        var dueDateText = $" (Due: {actionPoint.DueDate:yyyy-MM-dd})";
                        var checkbox = actionPoint.IsCompleted ? "[x]" : "[ ]";

                        sb.AppendLine($"- {checkbox} {taskText}{personText}{dueDateText}");
                    }

                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTool.Attributes;
using OfficeTool.Models;
using System.Linq;
using System.Text;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace OfficeTool.ViewModels;

[NavigationItem("Protocol", "\uE7C3", "Shell_Protocol", 1)]
public partial class ProtocolViewModel : ObservableRecipient
{
    [ObservableProperty]
    private Protocol _currentProtocol;

    public ProtocolViewModel()
    {
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

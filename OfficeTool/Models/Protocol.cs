using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTool.Models;

public partial class Protocol : ObservableObject
{
    public Protocol()
    {
        _meetingDate = DateTimeOffset.Now;
        Topics = new ObservableCollection<ProtocolTopic>();
    }

    /// <summary>
    /// Subject of the meeting/protocol
    /// </summary>
    [ObservableProperty]
    private string _subject = string.Empty;

    /// <summary>
    /// Date of the meeting
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset _meetingDate;

    /// <summary>
    /// Persons that attended the meeting
    /// </summary>
    [ObservableProperty]
    private string _attendees = string.Empty;

    /// <summary>
    /// Persons that couldn't attend the meeting
    /// </summary>
    [ObservableProperty]
    private string _absentees = string.Empty;

    /// <summary>
    /// Persons that were excused
    /// </summary>
    [ObservableProperty]
    private string _excused = string.Empty;

    /// <summary>
    /// Person leading the meeting
    /// </summary>
    private string _meetingLead = string.Empty;

    /// <summary>
    /// Person recording the minutes of the meeting
    /// </summary>
    private string _recorder = string.Empty;

    /// <summary>
    /// Person leading the meeting
    /// </summary>
    public string MeetingLead
    {
        get => _meetingLead;
        set => SetProperty(ref _meetingLead, value);
    }

    /// <summary>
    /// Person recording the minutes of the meeting
    /// </summary>
    public string Recorder
    {
        get => _recorder;
        set => SetProperty(ref _recorder, value);
    }

    /// <summary>
    /// Collection of meeting topics/points
    /// </summary>
    public ObservableCollection<ProtocolTopic> Topics
    {
        get; set;
    }
}

public partial class ProtocolTopic : ObservableObject
{
    public ProtocolTopic()
    {
        ActionPoints = new ObservableCollection<ActionPoint>();
    }

    /// <summary>
    /// Topic number (automatically assigned)
    /// </summary>
    [ObservableProperty]
    private int _number;

    /// <summary>
    /// Markdown content for this topic
    /// </summary>
    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>
    /// Optional title for the topic
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Collection of action points for this topic
    /// </summary>
    public ObservableCollection<ActionPoint> ActionPoints
    {
        get; set;
    }
}

public partial class ActionPoint : ObservableObject
{
    public ActionPoint()
    {
        _dueDate = DateTimeOffset.Now.AddDays(7); // Default to 1 week from now
    }

    /// <summary>
    /// Task description
    /// </summary>
    [ObservableProperty]
    private string _task = string.Empty;

    /// <summary>
    /// Person responsible for the task
    /// </summary>
    [ObservableProperty]
    private string _person = string.Empty;

    /// <summary>
    /// Due date for the task
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset _dueDate;

    /// <summary>
    /// Whether the task is completed
    /// </summary>
    [ObservableProperty]
    private bool _isCompleted;
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeTool.Attributes;
using OfficeTool.Models;
using System.Linq;

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
}

using Microsoft.UI.Xaml.Controls;

using OfficeTool.ViewModels;
using OfficeTool.Models;

namespace OfficeTool.Views;

public sealed partial class ProtocolPage : Page
{
    public ProtocolViewModel ViewModel
    {
        get;
    }

    public ProtocolPage()
    {
        ViewModel = App.GetService<ProtocolViewModel>();
        InitializeComponent();
    }

    private void DeleteTopicButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProtocolTopic topic)
        {
            ViewModel.RemoveTopicCommand.Execute(topic);
        }
    }

    private void AddActionPointButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProtocolTopic topic)
        {
            ViewModel.AddActionPointCommand.Execute(topic);
        }
    }

    private void DeleteActionPointButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ActionPoint actionPoint)
        {
            var topic = FindParentTopic(actionPoint);
            if (topic != null)
            {
                topic.ActionPoints.Remove(actionPoint);
            }
        }
    }

    private ProtocolTopic FindParentTopic(ActionPoint actionPoint)
    {
        foreach (var topic in ViewModel.CurrentProtocol.Topics)
        {
            if (topic.ActionPoints.Contains(actionPoint))
            {
                return topic;
            }
        }
        return null;
    }
}

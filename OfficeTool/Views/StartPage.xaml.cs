using Microsoft.UI.Xaml.Controls;

using OfficeTool.ViewModels;

namespace OfficeTool.Views;

public sealed partial class StartPage : Page
{
    public StartViewModel ViewModel
    {
        get;
    }

    public StartPage()
    {
        ViewModel = App.GetService<StartViewModel>();
        InitializeComponent();
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

using OfficeTool.Contracts.Services;
using OfficeTool.Helpers;
using OfficeTool.Models;
using OfficeTool.Views;

namespace OfficeTool.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    public ObservableCollection<NavigationMenuItem> NavigationMenuItems { get; } = new();
    public ObservableCollection<NavigationMenuItem> NavigationFooterMenuItems { get; } = new();

    public INavigationService NavigationService
    {
        get;
    }

    public INavigationViewService NavigationViewService
    {
        get;
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        LoadNavigationMenuItems();
    }

    private void LoadNavigationMenuItems()
    {
        NavigationMenuItems.Clear();
        NavigationFooterMenuItems.Clear();

        var mainItems = NavigationItemsHelper.GetMainNavigationItems();
        foreach (var item in mainItems)
        {
            NavigationMenuItems.Add(item);
        }

        var footerItems = NavigationItemsHelper.GetFooterNavigationItems();
        foreach (var item in footerItems)
        {
            NavigationFooterMenuItems.Add(item);
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }
    }
}

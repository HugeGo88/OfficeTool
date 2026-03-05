using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using OfficeTool.Attributes;
using OfficeTool.Contracts.Services;
using OfficeTool.Contracts.ViewModels;
using OfficeTool.Helpers;
using OfficeTool.Models;

namespace OfficeTool.ViewModels;

[NavigationItem("Start", "\uE80F", "Shell_Start", 0, isFooterItem: true)]
public partial class StartViewModel : ObservableRecipient, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ObservableCollection<NavigationMenuItem> Source { get; } = new ObservableCollection<NavigationMenuItem>();

    public StartViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        Source.Clear();

        var items = NavigationItemsHelper.GetMainNavigationItems();
        foreach (var item in items)
        {
            Source.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    private void OnItemClick(NavigationMenuItem? clickedItem)
    {
        if (clickedItem != null && !string.IsNullOrEmpty(clickedItem.PageKey))
        {
            _navigationService.NavigateTo(clickedItem.PageKey);
        }
    }
}

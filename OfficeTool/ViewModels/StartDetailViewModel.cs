using CommunityToolkit.Mvvm.ComponentModel;

using OfficeTool.Contracts.ViewModels;
using OfficeTool.Core.Contracts.Services;
using OfficeTool.Core.Models;

namespace OfficeTool.ViewModels;

public partial class StartDetailViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;

    [ObservableProperty]
    private SampleOrder? item;

    public StartDetailViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        if (parameter is long orderID)
        {
            var data = await _sampleDataService.GetStartDataAsync();
            Item = data.First(i => i.OrderID == orderID);
        }
    }

    public void OnNavigatedFrom()
    {
    }
}

using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using OfficeTool.Contracts.Services;
using OfficeTool.Helpers;

using Windows.ApplicationModel;
using Windows.Storage.Pickers;

namespace OfficeTool.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ICssStyleService _cssStyleService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string _customCss;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ICssStyleService cssStyleService)
    {
        _themeSelectorService = themeSelectorService;
        _cssStyleService = cssStyleService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _customCss = _cssStyleService.CustomCss;

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    partial void OnCustomCssChanged(string value)
    {
        _cssStyleService.SetCustomCssAsync(value).ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task LoadCssFromFile()
    {
        var picker = new FileOpenPicker();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".css");

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            CustomCss = await Windows.Storage.FileIO.ReadTextAsync(file);
        }
    }

    [RelayCommand]
    private async Task ClearCustomCss()
    {
        CustomCss = string.Empty;
        await _cssStyleService.SetCustomCssAsync(string.Empty);
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}

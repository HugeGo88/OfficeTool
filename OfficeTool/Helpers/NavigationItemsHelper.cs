using System.Reflection;
using OfficeTool.Attributes;
using OfficeTool.Models;
using OfficeTool.ViewModels;

namespace OfficeTool.Helpers;

public static class NavigationItemsHelper
{
    public static List<NavigationMenuItem> GetMainNavigationItems()
    {
        return GetNavigationItems(isFooter: false);
    }

    public static List<NavigationMenuItem> GetFooterNavigationItems()
    {
        return GetNavigationItems(isFooter: true);
    }

    private static List<NavigationMenuItem> GetNavigationItems(bool isFooter)
    {
        var navigationItems = new List<NavigationMenuItem>();

        var viewModelTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.Namespace == "OfficeTool.ViewModels" && t.IsClass && !t.IsAbstract);

        foreach (var viewModelType in viewModelTypes)
        {
            var attribute = viewModelType.GetCustomAttribute<NavigationItemAttribute>();
            if (attribute != null && attribute.IsFooterItem == isFooter)
            {
                var localizationKey = string.IsNullOrEmpty(attribute.LocalizationKey) 
                    ? $"Shell_{viewModelType.Name.Replace("ViewModel", "")}" 
                    : attribute.LocalizationKey;

                navigationItems.Add(new NavigationMenuItem
                {
                    Label = GetLocalizedOrDefault(localizationKey, attribute.Label),
                    Glyph = attribute.Glyph,
                    PageKey = viewModelType.FullName!
                });
            }
        }

        return navigationItems.OrderBy(x => 
        {
            var viewModelType = Assembly.GetExecutingAssembly().GetType(x.PageKey);
            return viewModelType?.GetCustomAttribute<NavigationItemAttribute>()?.Order ?? int.MaxValue;
        }).ToList();
    }

    private static string GetLocalizedOrDefault(string key, string defaultValue)
    {
        try
        {
            var localized = key.GetLocalized();
            return string.IsNullOrEmpty(localized) ? defaultValue : localized;
        }
        catch
        {
            return defaultValue;
        }
    }
}

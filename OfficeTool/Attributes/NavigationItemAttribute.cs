namespace OfficeTool.Attributes;

/// <summary>
/// Marks a ViewModel as a navigation item that should appear in the StartPage.
/// The navigation items are automatically discovered using reflection.
/// </summary>
/// <example>
/// [NavigationItem("MyPage", "\uE7C3", "Shell_MyPage", 10)]
/// public partial class MyPageViewModel : ObservableRecipient
/// {
///     // Your ViewModel implementation
/// }
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NavigationItemAttribute : Attribute
{
    public string Label { get; set; } = string.Empty;
    public string LocalizationKey { get; set; } = string.Empty;
    public string Glyph { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public bool IsFooterItem { get; set; } = false;

    public NavigationItemAttribute(string label, string glyph, int order = 0, bool isFooterItem = false)
    {
        Label = label;
        Glyph = glyph;
        Order = order;
        IsFooterItem = isFooterItem;
    }

    public NavigationItemAttribute(string label, string glyph, string localizationKey, int order = 0, bool isFooterItem = false)
    {
        Label = label;
        Glyph = glyph;
        LocalizationKey = localizationKey;
        Order = order;
        IsFooterItem = isFooterItem;
    }
}

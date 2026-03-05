# Navigation System - Reflection-Based Auto-Discovery

## Overview
The navigation system now uses reflection to automatically discover and display pages in both the StartPage and the ShellPage navigation menu. When you add a new page to your application, it will automatically appear without manual registration in multiple places.

## How to Add a New Page

### 1. Create Your ViewModel
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using OfficeTool.Attributes;

namespace OfficeTool.ViewModels;

[NavigationItem("MyPage", "\uE8A5", "Shell_MyPage", 10)]
public partial class MyPageViewModel : ObservableRecipient
{
    // Your implementation
}
```

### 2. Register in PageService
In `Services\PageService.cs`, add the mapping in the constructor:
```csharp
Configure<MyPageViewModel, MyPagePage>();
```

### 3. Done!
Your page will automatically appear on both:
- **StartPage** - As a tile with the icon and label
- **ShellPage Navigation Menu** - As a menu item with the icon and label

## NavigationItem Attribute Parameters

- **label** (string): The default display text (used if localization fails)
- **glyph** (string): The Segoe MDL2 Assets icon code (e.g., "\uE8A5")
- **localizationKey** (string, optional): The resource key for localized text
- **order** (int, optional): Display order (lower numbers appear first)
- **isFooterItem** (bool, optional): If true, item appears in navigation footer instead of main menu

### Footer Items Example
```csharp
[NavigationItem("Start", "\uE80F", "Shell_Start", 0, isFooterItem: true)]
public partial class StartViewModel : ObservableRecipient
{
    // This will appear in the footer of ShellPage navigation
}
```

## Common Segoe MDL2 Asset Icons

- `\uE70F` - Edit/Pencil
- `\uE716` - People/Contacts
- `\uE7C3` - View/Document
- `\uE8A5` - Calculator
- `\uE774` - Folder
- `\uE80F` - Home
- `\uE8F1` - Settings

Full icon reference: https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font

## Implementation Details

### NavigationItemsHelper
- `GetMainNavigationItems()` - Returns regular navigation items (IsFooterItem = false)
- `GetFooterNavigationItems()` - Returns footer navigation items (IsFooterItem = true)

Both methods:
1. Scan all ViewModels in the `OfficeTool.ViewModels` namespace using reflection
2. Find ViewModels decorated with `[NavigationItem]` attribute
3. Filter based on `IsFooterItem` property
4. Create NavigationMenuItem objects with localized labels and specified icons
5. Sort items by the Order property

### ShellPage Integration
The `ShellPage.xaml.cs` dynamically builds the NavigationView menu items in the `BuildNavigationMenu()` method, which:
- Populates `MenuItems` with regular navigation items
- Populates `FooterMenuItems` with footer navigation items
- Sets proper icons and navigation properties

### StartPage Integration
The `StartPage` displays all navigation items (except footer items) as tiles in an adaptive grid.

## Benefits
✅ **Single source of truth** - Define navigation once with the attribute  
✅ **Type-safe** - Uses ViewModel types for navigation  
✅ **Automatic synchronization** - Icons and labels stay in sync everywhere  
✅ **Easy maintenance** - Add/remove pages by just adding/removing the attribute  
✅ **Localization support** - Full support for resource strings

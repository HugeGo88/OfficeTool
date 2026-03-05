using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTool.Models;

public partial class Letter : ObservableObject
{
    public Letter()
    {
        // start new Letters with current date
        _setDate = DateTime.Now;
    }

    /// <summary>
    /// sets if there is a letter header or not
    /// </summary>
    [ObservableProperty]
    private bool _header;

    /// <summary>
    /// contrains the company name of the letter header
    /// </summary>
    [ObservableProperty]
    private string _company = string.Empty;

    /// <summary>
    /// subject of the letter
    /// </summary>
    [ObservableProperty]
    private string _subject = string.Empty;

    /// <summary>
    /// sets the first name of the letter
    /// </summary>
    [ObservableProperty]
    private string _firstName = string.Empty;

    /// <summary>
    /// sets the last name of the letter
    /// </summary>
    [ObservableProperty]
    private string _lastName = string.Empty;

    /// <summary>
    /// sets the street of the letter
    /// </summary>
    [ObservableProperty]
    private string _street = string.Empty;

    /// <summary>
    /// sets the city of the letter
    /// </summary>
    [ObservableProperty]
    private string _city = string.Empty;

    /// <summary>
    /// sets if there are signatures at the end of the letter or not
    /// </summary>
    [ObservableProperty]
    private bool _signing;

    /// <summary>
    /// actual date of the letter
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset _setDate;

    /// <summary>
    /// set a custom date for the letter or not
    /// </summary>
    [ObservableProperty]
    private bool _customeDate;

    /// <summary>
    /// content of the letter
    /// </summary>
    [ObservableProperty]
    private string _content = string.Empty;

    /// <summary>
    /// path of the letter
    /// </summary>
    [ObservableProperty]
    private string _path = string.Empty;
}

namespace OfficeTool.Contracts.Services;

public interface ICssStyleService
{
    string CustomCss { get; }

    Task InitializeAsync();

    Task SetCustomCssAsync(string css);

    string GetCss();

    string WrapInHtmlDocument(string bodyContent);
}

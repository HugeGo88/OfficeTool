using OfficeTool.Contracts.Services;

namespace OfficeTool.Services;

public class CssStyleService : ICssStyleService
{
    private const string SettingsKey = "CustomPdfCss";

    private readonly ILocalSettingsService _localSettingsService;

    public string CustomCss { get; private set; } = string.Empty;

    public CssStyleService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        CustomCss = await _localSettingsService.ReadSettingAsync<string>(SettingsKey) ?? string.Empty;
    }

    public async Task SetCustomCssAsync(string css)
    {
        CustomCss = css;
        await _localSettingsService.SaveSettingAsync(SettingsKey, css);
    }

    public string GetCss()
    {
        if (!string.IsNullOrWhiteSpace(CustomCss))
        {
            return CustomCss;
        }

        return GetDefaultCss();
    }

    public string WrapInHtmlDocument(string bodyContent)
    {
        var css = GetCss();
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
{css}
    </style>
</head>
<body>
<div class='page'>
    {bodyContent}
</div>
</body>
</html>";
    }

    private static string GetDefaultCss()
    {
        return @"
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background-color: #f0f0f0;
            padding: 20px;
            margin: 0;
        }
        .page {
            max-width: 210mm;
            min-height: 297mm;
            margin: 0 auto 20px auto;
            background-color: white;
            padding: 25mm;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            box-sizing: border-box;
            line-height: 1.6;
            color: #333;
        }
        h1, h2, h3, h4, h5, h6 {
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
            page-break-after: avoid;
            page-break-inside: avoid;
        }
        h1 {
            font-size: 2em;
            border-bottom: 1px solid #eaecef;
            padding-bottom: 0.3em;
        }
        h2 {
            font-size: 1.5em;
            border-bottom: 1px solid #eaecef;
            padding-bottom: 0.3em;
        }
        h3 { font-size: 1.25em; }
        h4 { font-size: 1em; }
        code {
            background-color: rgba(27, 31, 35, 0.05);
            border-radius: 3px;
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 85%;
            padding: 0.2em 0.4em;
        }
        pre {
            background-color: #f6f8fa;
            border-radius: 3px;
            padding: 16px;
            overflow: auto;
        }
        pre code {
            background-color: transparent;
            padding: 0;
        }
        blockquote {
            border-left: 4px solid #dfe2e5;
            color: #6a737d;
            padding: 0 1em;
            margin: 0;
        }
        table {
            border-collapse: collapse;
            width: 100%;
        }
        table th, table td {
            border: 1px solid #dfe2e5;
            padding: 6px 13px;
        }
        table tr:nth-child(2n) {
            background-color: #f6f8fa;
        }
        a {
            color: #0366d6;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        img {
            max-width: 100%;
        }
        @page {
            size: A4;
            margin: 0%;
        }
        @media print {
            body {
                background-color: white;
                padding: 0;
                margin: 0;
            }
            .page {
                max-width: 210mm;
                min-height: 297mm;
                margin: 0;
                background-color: white;
                padding: 25mm;
                box-shadow: none;
                box-sizing: border-box;
                line-height: 1.6;
                color: #333;
            }
            table th, table td {
                border-color: #000;
            }
            table tr:nth-child(2n) {
                background-color: #f6f8fa;
            }
            a {
                color: #0366d6;
            }
        }
        @media (prefers-color-scheme: dark) {
            body {
                background-color: #0d0d0d;
            }
            .page {
                background-color: #1e1e1e;
                color: #d4d4d4;
                box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
            }
            h1, h2 {
                border-bottom-color: #444;
            }
            code {
                background-color: rgba(255, 255, 255, 0.1);
            }
            pre {
                background-color: #2d2d2d;
            }
            blockquote {
                border-left-color: #444;
                color: #999;
            }
            table th, table td {
                border-color: #444;
            }
            table tr:nth-child(2n) {
                background-color: #2d2d2d;
            }
            a {
                color: #58a6ff;
            }
        }";
    }
}

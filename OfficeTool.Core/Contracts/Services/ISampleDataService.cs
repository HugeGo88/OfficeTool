using OfficeTool.Core.Models;

namespace OfficeTool.Core.Contracts.Services;

// Remove this class once your pages/features are using your data.
public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetStartDataAsync();

    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}

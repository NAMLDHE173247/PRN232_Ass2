using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Report;

namespace ass01.BusinessLogic.Services;

public interface IReportService
{
    Task<ReportResponse> GetReportAsync(ReportRequest request);
}

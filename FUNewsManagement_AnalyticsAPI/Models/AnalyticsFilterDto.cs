using System;

namespace FUNewsManagement_AnalyticsAPI.Models;

public class AnalyticsFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public short? CategoryId { get; set; }
    public bool? Status { get; set; }
    public short? AuthorId { get; set; }
}

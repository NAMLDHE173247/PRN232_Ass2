using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ass01_FE.Presentation.Models.News;

public class ODataResponse<T>
{
    [JsonPropertyName("@odata.count")]
    public int Count { get; set; }

    [JsonPropertyName("value")]
    public List<T>? Value { get; set; }
}

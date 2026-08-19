using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ass01_FE.Infrastructure.Clients;

public class AiApiClient
{
    public HttpClient Client { get; }

    public AiApiClient(HttpClient client)
    {
        Client = client;
    }

    public async Task<SuggestTagsResponse?> SuggestTagsAsync(string content)
    {
        var response = await Client.PostAsJsonAsync("api/ai/suggest-tags", new { Content = content });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SuggestTagsResponse>();
        }
        return null; // Gracefully handle failure
    }

    public async Task<bool> RecordSelectedSuggestedTagsAsync(string keyword, string tagName)
    {
        var response = await Client.PostAsJsonAsync("api/ai/learn", new { Keyword = keyword, TagName = tagName });
        return response.IsSuccessStatusCode;
    }
}

public class SuggestTagsResponse
{
    public List<SuggestedTagDto> Tags { get; set; } = new();
}

public class SuggestedTagDto
{
    public string Name { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

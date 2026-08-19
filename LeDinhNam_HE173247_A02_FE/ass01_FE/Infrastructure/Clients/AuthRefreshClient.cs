using System.Net.Http;

namespace ass01_FE.Infrastructure.Clients;

public class AuthRefreshClient
{
    public HttpClient Client { get; }

    public AuthRefreshClient(HttpClient client)
    {
        Client = client;
    }
}

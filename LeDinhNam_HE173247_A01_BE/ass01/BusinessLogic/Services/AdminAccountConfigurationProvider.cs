using Microsoft.Extensions.Configuration;

namespace ass01.BusinessLogic.Services;

public class AdminAccountConfigurationProvider
{
    private readonly IConfiguration _configuration;

    public AdminAccountConfigurationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string AdminEmail => _configuration["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
    public string AdminPassword => _configuration["AdminAccount:Password"] ?? "@@abc123@@";
}

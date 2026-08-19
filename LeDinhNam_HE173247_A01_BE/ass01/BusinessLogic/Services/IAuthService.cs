using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Auth;

namespace ass01.BusinessLogic.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshAsync(string refreshToken);
}


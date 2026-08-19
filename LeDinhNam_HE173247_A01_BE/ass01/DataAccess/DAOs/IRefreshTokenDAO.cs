using System.Threading.Tasks;
using ass01.Models;

namespace ass01.DataAccess.DAOs;

public interface IRefreshTokenDAO
{
    Task SaveAsync(RefreshToken token);
    Task<RefreshToken?> GetValidTokenAsync(string tokenHash);
    Task RevokeAsync(int tokenId);
    Task RevokeAllForAccountAsync(short accountId);
    Task RotateRefreshTokenAsync(int oldTokenId, RefreshToken newToken);
}

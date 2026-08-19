using System.Threading.Tasks;
using ass01.DataAccess.DAOs;
using ass01.Models;

namespace ass01.DataAccess.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IRefreshTokenDAO _dao;

    public RefreshTokenRepository(IRefreshTokenDAO dao)
    {
        _dao = dao;
    }

    public Task SaveAsync(RefreshToken token) => _dao.SaveAsync(token);

    public Task<RefreshToken?> GetValidTokenAsync(string tokenHash) => _dao.GetValidTokenAsync(tokenHash);

    public Task RevokeAsync(int tokenId) => _dao.RevokeAsync(tokenId);

    public Task RevokeAllForAccountAsync(short accountId) => _dao.RevokeAllForAccountAsync(accountId);

    public Task RotateRefreshTokenAsync(int oldTokenId, RefreshToken newToken) => _dao.RotateRefreshTokenAsync(oldTokenId, newToken);
}

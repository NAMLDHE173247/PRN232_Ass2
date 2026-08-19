using System;
using System.Linq;
using System.Threading.Tasks;
using ass01.Models;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class RefreshTokenDAO : IRefreshTokenDAO
{
    private readonly FunewsManagementContext _context;

    public RefreshTokenDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r =>
                r.Token == tokenHash &&
                r.RevokedAt == null &&
                r.ExpiresAt > DateTime.UtcNow);
    }

    public async Task RevokeAsync(int tokenId)
    {
        var token = await _context.RefreshTokens.FindAsync(tokenId);
        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllForAccountAsync(short accountId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.AccountId == accountId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task RotateRefreshTokenAsync(int oldTokenId, RefreshToken newToken)
    {
        var oldToken = await _context.RefreshTokens.FindAsync(oldTokenId);
        if (oldToken != null)
        {
            oldToken.RevokedAt = DateTime.UtcNow;
        }

        await _context.RefreshTokens.AddAsync(newToken);
        
        // EF Core SaveChangesAsync handles both operations within a single transaction automatically
        await _context.SaveChangesAsync();
    }
}

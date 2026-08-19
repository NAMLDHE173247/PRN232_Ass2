using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ass01.Models;
using Microsoft.EntityFrameworkCore;

namespace ass01.DataAccess.DAOs;

public class AccountDAO : IAccountDAO
{
    private readonly FunewsManagementContext _context;

    public AccountDAO(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<SystemAccount?> GetAccountByEmailAsync(string email)
    {
        return await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountEmail == email);
    }

    public async Task<List<SystemAccount>> GetAccountsAsync()
    {
        return await _context.SystemAccounts.ToListAsync();
    }

    public async Task<SystemAccount?> GetAccountByIdAsync(short id)
    {
        return await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountId == id);
    }

    public async Task AddAccountAsync(SystemAccount account)
    {
        await _context.SystemAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAccountAsync(SystemAccount account)
    {
        _context.SystemAccounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(SystemAccount account)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.AccountId == account.AccountId).ToListAsync();
        if (tokens.Any())
        {
            _context.RefreshTokens.RemoveRange(tokens);
        }
        
        _context.SystemAccounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasCreatedNewsAsync(short accountId)
    {
        return await _context.NewsArticles
            .AnyAsync(n => n.CreatedById == accountId);
    }
}

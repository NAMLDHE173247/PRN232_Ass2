using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.Models;

namespace ass01.DataAccess.DAOs;

public interface IAccountDAO
{
    Task<SystemAccount?> GetAccountByEmailAsync(string email);
    Task<List<SystemAccount>> GetAccountsAsync();
    Task<SystemAccount?> GetAccountByIdAsync(short id);
    Task AddAccountAsync(SystemAccount account);
    Task UpdateAccountAsync(SystemAccount account);
    Task DeleteAccountAsync(SystemAccount account);
    Task<bool> HasCreatedNewsAsync(short accountId);
}

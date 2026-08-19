using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.Models;
using ass01.DataAccess.DAOs;

namespace ass01.DataAccess.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IAccountDAO _dao;

    public AccountRepository(IAccountDAO dao)
    {
        _dao = dao;
    }

    public Task<SystemAccount?> GetAccountByEmailAsync(string email)
        => _dao.GetAccountByEmailAsync(email);

    public Task<List<SystemAccount>> GetAccountsAsync()
        => _dao.GetAccountsAsync();

    public Task<SystemAccount?> GetAccountByIdAsync(short id)
        => _dao.GetAccountByIdAsync(id);

    public Task AddAccountAsync(SystemAccount account)
        => _dao.AddAccountAsync(account);

    public Task UpdateAccountAsync(SystemAccount account)
        => _dao.UpdateAccountAsync(account);

    public Task DeleteAccountAsync(SystemAccount account)
        => _dao.DeleteAccountAsync(account);

    public Task<bool> HasCreatedNewsAsync(short accountId)
        => _dao.HasCreatedNewsAsync(accountId);
}

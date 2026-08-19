using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Account;

namespace ass01.BusinessLogic.Services;

public interface IAccountService
{
    Task<List<AccountDto>> GetAccountsAsync(string? searchKeyword, string? roleFilter);
    Task<AccountDto?> GetAccountByIdAsync(short id);
    Task<AccountDto> CreateAccountAsync(CreateAccountRequest request);
    Task UpdateAccountAsync(short id, UpdateAccountRequest request);
    Task DeleteAccountAsync(short id);
    Task ChangePasswordAsync(short id, ChangePasswordRequest request);
}

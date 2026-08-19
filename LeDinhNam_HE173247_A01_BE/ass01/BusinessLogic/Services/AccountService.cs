using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Account;
using ass01.DataAccess.Repositories;
using ass01.Models;

namespace ass01.BusinessLogic.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IAuditLogService _auditLogService;

    public AccountService(IAccountRepository repository, IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<List<AccountDto>> GetAccountsAsync(string? searchKeyword, string? roleFilter)
    {
        var accounts = await _repository.GetAccountsAsync();
        
        // Simple manual mapping and filtering using straightforward LINQ
        var query = accounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            var keyword = searchKeyword.ToLower();
            query = query.Where(a => 
                (a.AccountName != null && a.AccountName.ToLower().Contains(keyword)) ||
                (a.AccountEmail != null && a.AccountEmail.ToLower().Contains(keyword)) ||
                (a.AccountRole.HasValue && a.AccountRole.ToString() == keyword)
            );
        }

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            if (int.TryParse(roleFilter, out int role))
            {
                query = query.Where(a => a.AccountRole == role);
            }
        }

        return query.Select(a => new AccountDto
        {
            AccountId = a.AccountId,
            AccountName = a.AccountName,
            AccountEmail = a.AccountEmail,
            AccountRole = a.AccountRole
        }).ToList();
    }

    public async Task<AccountDto?> GetAccountByIdAsync(short id)
    {
        var account = await _repository.GetAccountByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        return new AccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };
    }

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        // Business Rule: Duplicate Email forbidden
        var existing = await _repository.GetAccountByEmailAsync(request.AccountEmail);
        if (existing != null)
        {
            throw new ArgumentException("An account with this email already exists.");
        }

        var allAccounts = await _repository.GetAccountsAsync();
        short newId = allAccounts.Any() ? (short)(allAccounts.Max(a => a.AccountId) + 1) : (short)1;

        var account = new SystemAccount
        {
            AccountId = newId,
            AccountName = request.AccountName,
            AccountEmail = request.AccountEmail,
            AccountRole = request.AccountRole,
            AccountPassword = request.AccountPassword
        };

        await _repository.AddAccountAsync(account);

        var dto = new AccountDto
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };

        await _auditLogService.LogAsync("CREATE", "Account", account.AccountId.ToString(), null, dto);

        return dto;
    }

    public async Task UpdateAccountAsync(short id, UpdateAccountRequest request)
    {
        var account = await _repository.GetAccountByIdAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException("Account not found.");
        }

        // Business Rule: Duplicate Email forbidden (if changed)
        if (account.AccountEmail != request.AccountEmail)
        {
            var existing = await _repository.GetAccountByEmailAsync(request.AccountEmail);
            if (existing != null)
            {
                throw new ArgumentException("An account with this email already exists.");
            }
        }

        // Snapshot before update (Exclude password)
        var beforeSnapshot = new
        {
            account.AccountId,
            account.AccountName,
            account.AccountEmail,
            account.AccountRole
        };

        account.AccountName = request.AccountName;
        account.AccountEmail = request.AccountEmail;
        account.AccountRole = request.AccountRole;

        await _repository.UpdateAccountAsync(account);

        // Snapshot after update (Exclude password)
        var afterSnapshot = new
        {
            account.AccountId,
            account.AccountName,
            account.AccountEmail,
            account.AccountRole
        };

        await _auditLogService.LogAsync("UPDATE", "Account", account.AccountId.ToString(), beforeSnapshot, afterSnapshot);
    }

    public async Task DeleteAccountAsync(short id)
    {
        var account = await _repository.GetAccountByIdAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException("Account not found.");
        }

        // Business Rule: Cannot delete account if it has created news articles
        var hasNews = await _repository.HasCreatedNewsAsync(id);
        if (hasNews)
        {
            throw new InvalidOperationException("Cannot delete this account because it has created news articles.");
        }

        // Snapshot before delete (Exclude password)
        var beforeSnapshot = new
        {
            account.AccountId,
            account.AccountName,
            account.AccountEmail,
            account.AccountRole
        };

        await _repository.DeleteAccountAsync(account);

        await _auditLogService.LogAsync("DELETE", "Account", account.AccountId.ToString(), beforeSnapshot, null);
    }

    public async Task ChangePasswordAsync(short id, ChangePasswordRequest request)
    {
        var account = await _repository.GetAccountByIdAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException("Account not found.");
        }

        // Business Rule: Password change requires current password verification
        if (account.AccountPassword != request.CurrentPassword)
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        // Snapshot before update
        var beforeSnapshot = new
        {
            account.AccountId,
            account.AccountName,
            account.AccountEmail,
            account.AccountRole,
            PasswordChanged = false
        };

        account.AccountPassword = request.NewPassword;
        await _repository.UpdateAccountAsync(account);

        // Snapshot after update
        var afterSnapshot = new
        {
            account.AccountId,
            account.AccountName,
            account.AccountEmail,
            account.AccountRole,
            PasswordChanged = true
        };

        await _auditLogService.LogAsync("UPDATE", "Account", account.AccountId.ToString(), beforeSnapshot, afterSnapshot);
    }
}


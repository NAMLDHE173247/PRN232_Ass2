using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Auth;
using ass01.DataAccess.Repositories;
using ass01.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ass01.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    // Access token lifetime
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(1);
    // Refresh token lifetime
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    public AuthService(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // All authentication now goes through DB – Admin is seeded as AccountRole = 0
        var account = await _accountRepository.GetAccountByEmailAsync(request.Email);

        if (account == null || account.AccountPassword != request.Password)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        string role = account.AccountRole switch
        {
            0 => "Admin",
            1 => "Staff",
            2 => "Lecturer",
            _ => "Unknown"
        };

        if (role == "Unknown")
        {
            throw new UnauthorizedAccessException("Account has an invalid role.");
        }

        return await BuildLoginResponseAsync(account, role);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshTokenValue)
    {
        var tokenHash = HashToken(refreshTokenValue);
        var storedToken = await _refreshTokenRepository.GetValidTokenAsync(tokenHash);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid, expired, or revoked refresh token.");
        }

        var account = storedToken.Account;

        string role = account.AccountRole switch
        {
            0 => "Admin",
            1 => "Staff",
            2 => "Lecturer",
            _ => throw new UnauthorizedAccessException("Account has an invalid role.")
        };

        var expiresAt = DateTime.UtcNow.Add(AccessTokenLifetime);
        var accessToken = GenerateJwtToken(account.AccountEmail ?? string.Empty, role, account.AccountId, expiresAt);
        var newRefreshTokenRaw = GenerateSecureRefreshToken();
        var newRefreshTokenHash = HashToken(newRefreshTokenRaw);

        var newToken = new RefreshToken
        {
            AccountId = account.AccountId,
            Token = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        // Atomic rotation
        await _refreshTokenRepository.RotateRefreshTokenAsync(storedToken.Id, newToken);

        return new LoginResponse
        {
            Token = accessToken,
            RefreshToken = newRefreshTokenRaw,
            ExpiresAt = expiresAt,
            Email = account.AccountEmail ?? string.Empty,
            Role = role,
            AccountId = account.AccountId
        };
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<LoginResponse> BuildLoginResponseAsync(SystemAccount account, string role)
    {
        var expiresAt = DateTime.UtcNow.Add(AccessTokenLifetime);
        var accessToken = GenerateJwtToken(account.AccountEmail ?? string.Empty, role, account.AccountId, expiresAt);
        var refreshTokenRaw = GenerateSecureRefreshToken();
        var refreshTokenHash = HashToken(refreshTokenRaw);

        await _refreshTokenRepository.SaveAsync(new RefreshToken
        {
            AccountId = account.AccountId,
            Token = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        });

        return new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshTokenRaw,
            ExpiresAt = expiresAt,
            Email = account.AccountEmail ?? string.Empty,
            Role = role,
            AccountId = account.AccountId
        };
    }

    private string GenerateJwtToken(string email, string role, short accountId, DateTime expiresAt)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

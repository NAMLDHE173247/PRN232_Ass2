using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Account;
using ass01.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ass01.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccounts([FromQuery] string? searchKeyword, [FromQuery] string? roleFilter)
    {
        var accounts = await _accountService.GetAccountsAsync(searchKeyword, roleFilter);
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccountById(short id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null)
        {
            return NotFound(new { message = "Account not found." });
        }

        return Ok(account);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccountById), new { id = created.AccountId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount(short id, [FromBody] UpdateAccountRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _accountService.UpdateAccountAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Account not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(short id)
    {
        try
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Account not found." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/change-password")]
    [Authorize(Roles = "Staff,Lecturer")]
    public async Task<IActionResult> ChangePassword(short id, [FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!short.TryParse(userIdStr, out short claimId) || claimId != id)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _accountService.ChangePasswordAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Account not found." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ass01.BusinessLogic.DTOs.Account;
using ass01.BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ass01.Presentation.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(Roles = "Staff")]
public class ProfileController : ControllerBase
{
    private readonly IAccountService _accountService;

    public ProfileController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!short.TryParse(userIdStr, out short currentUserId))
        {
            return Unauthorized();
        }

        var account = await _accountService.GetAccountByIdAsync(currentUserId);
        if (account == null)
        {
            return NotFound(new { message = "Account not found." });
        }

        return Ok(account);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!short.TryParse(userIdStr, out short currentUserId))
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var account = await _accountService.GetAccountByIdAsync(currentUserId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            var updateReq = new UpdateAccountRequest
            {
                AccountName = request.AccountName,
                AccountEmail = request.AccountEmail,
                AccountRole = account.AccountRole ?? 1 // Preserve existing role
            };

            await _accountService.UpdateAccountAsync(currentUserId, updateReq);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

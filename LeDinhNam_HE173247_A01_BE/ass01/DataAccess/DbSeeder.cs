using System.Threading.Tasks;
using ass01.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ass01.DataAccess;

/// <summary>
/// Seeds required data into the database on application startup.
/// Safe to call multiple times (idempotent).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FunewsManagementContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FunewsManagementContext>>();

        try
        {
            await SeedAdminAsync(context, config, logger);
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Database seeding failed. Application will continue.");
        }
    }

    private static async Task SeedAdminAsync(
        FunewsManagementContext context,
        IConfiguration config,
        ILogger logger)
    {
        var adminEmail = config["AdminAccount:Email"] ?? "admin@FUNewsManagementSystem.org";
        var adminPassword = config["AdminAccount:Password"] ?? "@@abc123@@";
        var adminName = config["AdminAccount:Name"] ?? "System Admin";

        // Check if Admin already exists in DB (by role=0)
        var existingAdmin = await context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountRole == 0);

        if (existingAdmin != null)
        {
            bool updated = false;
            if (existingAdmin.AccountEmail != adminEmail) { existingAdmin.AccountEmail = adminEmail; updated = true; }
            if (existingAdmin.AccountName != adminName) { existingAdmin.AccountName = adminName; updated = true; }
            if (existingAdmin.AccountPassword != adminPassword) { existingAdmin.AccountPassword = adminPassword; updated = true; }

            if (updated)
            {
                context.SystemAccounts.Update(existingAdmin);
                await context.SaveChangesAsync();
                logger.LogInformation("Existing admin account UPSERTed with latest config.");
            }
            else
            {
                logger.LogInformation("Admin account already exists in DB (AccountId={Id}). Skipping seed.",
                    existingAdmin.AccountId);
            }
            return;
        }

        // Find a safe AccountId (9999 if free, else find next free from 9998 downward)
        short adminId = 9999;
        while (await context.SystemAccounts.AnyAsync(a => a.AccountId == adminId))
        {
            adminId--;
        }

        var admin = new SystemAccount
        {
            AccountId = adminId,
            AccountName = adminName,
            AccountEmail = adminEmail,
            AccountRole = 0,       // 0 = Admin
            AccountPassword = adminPassword
        };

        await context.SystemAccounts.AddAsync(admin);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Admin account seeded to DB: AccountId={Id}, Email={Email}, Role=0.",
            adminId, adminEmail);
    }
}

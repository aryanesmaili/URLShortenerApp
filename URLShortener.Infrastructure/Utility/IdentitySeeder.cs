using Microsoft.AspNetCore.Identity;
using URLShortener.Application.Models;

namespace URLShortener.Infrastructure.Utility;

public static class IdentitySeeder
{
    public static async Task SeedRoles(RoleManager<AppRole> roleManager)
    {
        Dictionary<string, string> roleNames = new()
        {
            {"Admin", "Administrator with full access"},
            {"User", "Regular user with limited access"},
            {"TelegramBot", "Bot for Telegram integration"}
        };
        for (int i = 0; i < roleNames.Count; i++)
        {
            var item = roleNames.ElementAt(i);
            if (!await roleManager.RoleExistsAsync(item.Key))
            {
                await roleManager.CreateAsync(new AppRole { Name = item.Key, Description = item.Value });
            }
        }
    }
}

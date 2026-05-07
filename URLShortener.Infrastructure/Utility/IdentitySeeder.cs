using Microsoft.AspNetCore.Identity;
using URLShortener.Application.Common.Models.Identity;
using URLShortener.Application.Configuration;

namespace URLShortener.Infrastructure.Utility;

public static class IdentitySeeder
{
    public static async Task SeedRoles(RoleManager<AppRole> roleManager, AuthorizationSettings authorizationSettings)
    {
        foreach (var role in authorizationSettings.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Key))
                await roleManager.CreateAsync(new AppRole { Name = role.Key, Description = role.Value });
        }
    }
}

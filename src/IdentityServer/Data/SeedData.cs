using IdentityServer.Configuration;
using IdentityServer.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityServer.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager)
    {
        var configurationDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>();
        
        // Seed IdentityServer configuration
        if (!configurationDbContext.Clients.Any())
        {
            foreach (var client in Config.Clients)
            {
                configurationDbContext.Clients.Add(client.ToEntity());
            }
            await configurationDbContext.SaveChangesAsync();
        }

        if (!configurationDbContext.IdentityResources.Any())
        {
            foreach (var resource in Config.IdentityResources)
            {
                configurationDbContext.IdentityResources.Add(resource.ToEntity());
            }
            await configurationDbContext.SaveChangesAsync();
        }

        if (!configurationDbContext.ApiScopes.Any())
        {
            foreach (var apiScope in Config.ApiScopes)
            {
                configurationDbContext.ApiScopes.Add(apiScope.ToEntity());
            }
            await configurationDbContext.SaveChangesAsync();
        }

        if (!configurationDbContext.ApiResources.Any())
        {
            foreach (var resource in Config.ApiResources)
            {
                configurationDbContext.ApiResources.Add(resource.ToEntity());
            }
            await configurationDbContext.SaveChangesAsync();
        }

        // Seed Roles
        await SeedRolesAsync(roleManager);
        
        // Seed Users
        await SeedUsersAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "User", "Manager" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Admin User
        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                await userManager.AddClaimsAsync(adminUser, new[]
                {
                    new Claim("given_name", adminUser.FirstName ?? ""),
                    new Claim("family_name", adminUser.LastName ?? ""),
                    new Claim("email", adminUser.Email ?? ""),
                    new Claim("role", "Admin")
                });
            }
        }

        // Regular User
        var userEmail = "user@example.com";
        var regularUser = await userManager.FindByEmailAsync(userEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            var result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
                await userManager.AddClaimsAsync(regularUser, new[]
                {
                    new Claim("given_name", regularUser.FirstName ?? ""),
                    new Claim("family_name", regularUser.LastName ?? ""),
                    new Claim("email", regularUser.Email ?? ""),
                    new Claim("role", "User")
                });
            }
        }

        // Manager User
        var managerEmail = "manager@example.com";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true
            };

            var result = await userManager.CreateAsync(managerUser, "Manager123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
                await userManager.AddClaimsAsync(managerUser, new[]
                {
                    new Claim("given_name", managerUser.FirstName ?? ""),
                    new Claim("family_name", managerUser.LastName ?? ""),
                    new Claim("email", managerUser.Email ?? ""),
                    new Claim("role", "Manager")
                });
            }
        }
    }
} 
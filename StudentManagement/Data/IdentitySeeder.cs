using Microsoft.AspNetCore.Identity;
using StudentManagement.Models;

namespace StudentManagement.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            foreach (var roleName in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var existingAdmins = await userManager.GetUsersInRoleAsync(Roles.Admin);
            if (existingAdmins.Count > 0)
            {
                return;
            }

            var adminEmail = configuration["DefaultAdmin:Email"];
            var adminPassword = configuration["DefaultAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "No Admin user exists and 'DefaultAdmin:Email' / 'DefaultAdmin:Password' are not configured. " +
                    "Set them via user-secrets before running the app.");
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrator",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to seed default Admin user: {errors}");
            }

            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}

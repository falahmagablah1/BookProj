using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Create roles
            string[] roles = { "Admin", "Editor", "User" };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create default admin user
            string adminEmail = "admin@bookmanagement.com";
            string adminPassword = "Admin123!";

            if (userManager.FindByEmailAsync(adminEmail).Result == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create default editor user
            string editorEmail = "editor@bookmanagement.com";
            string editorPassword = "Editor123!";

            if (userManager.FindByEmailAsync(editorEmail).Result == null)
            {
                var editorUser = new ApplicationUser
                {
                    UserName = editorEmail,
                    Email = editorEmail,
                    FirstName = "Content",
                    LastName = "Editor",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(editorUser, editorPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(editorUser, "Editor");
                }
            }
        }
    }
}
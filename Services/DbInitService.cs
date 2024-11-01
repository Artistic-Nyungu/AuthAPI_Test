using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ContosoPizza.Services;

public static class DbInitService{

    public static IApplicationBuilder AddAdmin(this WebApplication app, string username = "admin_alan", string password = "alan222s", string? email=null)
    {
        var serviceProvider = app.Services.CreateScope().ServiceProvider;
        var context = serviceProvider.GetRequiredService<AuthDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.EnsureCreated();

        var admin = new IdentityUser()
        {
            UserName = username,
            Email = email ?? username + "@example.com",
        };

        if(userManager.Users.Any(u => u.UserName == admin.UserName))
        {
            app.Logger.LogError($"Admin {admin.UserName} already exists");
            return app;
        }
        var userTask = userManager.CreateAsync(admin, password);
        userTask.Wait();

        if(!roleManager.Roles.Any(r => r.Name == UserRoles.Admin))
        {
            var roleTask = roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            roleTask.Wait();
        }

        var assignRoleTask = userManager.AddToRoleAsync(admin, UserRoles.Admin);
        assignRoleTask.Wait();

        if(!assignRoleTask.Result.Succeeded)
        {
            app.Logger.LogError(string.Join(Environment.NewLine, assignRoleTask.Result.Errors.Select(err => $"Error {err.Code}: {string.Join(Environment.NewLine, err.Description.Split(Environment.NewLine).Select(line => "\t" + line).ToArray())}").ToArray()));
            return app;
        }

        app.Logger.LogInformation($"{username} registered as {UserRoles.Admin}");

        return app;
    }
}
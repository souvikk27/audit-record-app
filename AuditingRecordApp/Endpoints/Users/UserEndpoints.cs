using AuditingRecordApp.Endpoints.Users;
using AuditingRecordApp.Entity;
using AuditingRecordApp.Services;
using Microsoft.AspNetCore.Identity;

namespace AuditingRecordApp.Endpoints.Users;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints, IConfiguration configuration)
    {
        endpoints.MapPost("/api/users/register",
            async (UserManager<ApplicationUser> userManager, UserRegisterParameter parameter) =>
            {
                var user = new ApplicationUser
                {
                    FirstName = parameter.FirstName,
                    LastName = parameter.LastName,
                    UserName = parameter.UserName,
                    Email = parameter.Email,
                    EmailConfirmed = true,
                    IsAdmin = false
                };

                var result = await userManager.CreateAsync(user, parameter.Password);

                if (!result.Succeeded)
                {
                    return Results.BadRequest(result.Errors.ToString());
                }

                return Results.Ok();
            });
        
        endpoints.MapPost("/api/users/login",
            async (UserManager<ApplicationUser> userManager, 
                SignInManager<ApplicationUser> signInManager, 
                UserLoginParameter parameter) =>
            {
                var result = await signInManager.PasswordSignInAsync(
                    parameter.UserName, 
                    parameter.Password, 
                    false, 
                    false);
                if (!result.Succeeded)
                {
                    return Results.BadRequest(result.ToString());
                }
                
                var token = new JwtTokenProvider(configuration)
                    .GetToken(userManager.Users.First());
                return Results.Ok(token);
            });

        return endpoints;
    }
}
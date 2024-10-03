using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using AuditingRecordApp.Interceptors;
using AuditingRecordApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

namespace AuditingRecordApp.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentSessionProvider, CurrentSessionProvider>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        var connectionString = configuration.GetConnectionString("SqlConnection") ??
                               throw new ArgumentNullException(nameof(configuration));

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();


        services.AddDbContext<ApplicationDbContext>((provider, options) =>
        {
            var interceptor = provider.GetService<AuditSaveChangesInterceptor>()
                              ?? throw new NullReferenceException(nameof(AuditSaveChangesInterceptor));

            options.EnableSensitiveDataLogging()
                .UseNpgsql(dataSourceBuilder.Build())
                .AddInterceptors(interceptor)
                .UseSnakeCaseNamingConvention();
        });
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
                options.SignIn.RequireConfirmedAccount = false
            )
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["AuthConfiguration:Issuer"],
                    ValidAudience = configuration["AuthConfiguration:Audience"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AuthConfiguration:Key"]!))
                };
            });

        services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>();
    }
}
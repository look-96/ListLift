using ListLift.Infrastructure.Identity;
using ListLift.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ListLift.API.Authentication;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddListLiftAuthentication(this IServiceCollection services)
    {
        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 15;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ListingDbContext>();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ListLift.Application.Abstractions.ICurrentUser, HttpCurrentUser>();
        return services;
    }
}

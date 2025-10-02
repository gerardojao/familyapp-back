using FamilyApp.Application.Abstractions;
using FamilyApp.Application.Abstractions.Data;
using FamilyApp.Infrastructure.Email;
using FamilyApp.Infrastructure.Identity;
using FamilyApp.Infrastructure.Persistence;
using FamilyApp.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddDbContext<FamilyAppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRepository, Repository>();
        services.AddSingleton<IEmailSender, DevEmailSender>();

        return services;
    }
}

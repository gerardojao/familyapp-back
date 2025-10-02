using FamilyApp.Application.Abstractions;
using FamilyApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        return services;
    }
}

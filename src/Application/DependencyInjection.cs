using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Application.Common.Behaviors;

namespace Application;

public static class DependencyInjection
{
    /// <summary>
    /// MediatR lisans anahtarını opsiyonel parametre olarak alır.
    /// Program.cs içinden builder.Services.AddApplicationServices(licenseKey) ile veriyoruz.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        string? mediatrLicenseKey = null)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrWhiteSpace(mediatrLicenseKey))
                cfg.LicenseKey = mediatrLicenseKey;
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

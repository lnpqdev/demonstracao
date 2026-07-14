using System.Reflection;
using BffDemo.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BffDemo.Application;

/// <summary>
/// Cada camada expõe seu próprio "AddXxx" para registrar seus serviços no
/// contêiner de DI. Assim a Api (composition root) compõe as camadas sem
/// conhecer os detalhes internos de cada uma.
///
/// Este método registra tudo o que a Application precisa: MediatR (handlers
/// + pipeline behaviors) e os validators do FluentValidation.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // ORDEM IMPORTA: valida primeiro (barra cedo), depois loga.
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}

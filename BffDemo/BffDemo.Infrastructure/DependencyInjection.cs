using BffDemo.Application.Abstractions;
using BffDemo.Infrastructure.BrasilApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BffDemo.Infrastructure;

/// <summary>
/// Composição da camada de Infraestrutura. Aqui ligamos a PORT
/// (IEnderecoProvider) ao ADAPTER concreto (BrasilApiClient) e configuramos
/// o Typed HttpClient. A URL vem da configuração (appsettings), não do código.
///
/// Este é o único lugar onde a escolha "usamos a BrasilAPI" é feita. Trocar
/// por ViaCEP = novo adapter + mudar esta linha. Nada mais na aplicação muda.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IEnderecoProvider, BrasilApiClient>(client =>
        {
            var baseUrl = configuration["BrasilApi:BaseUrl"]
                          ?? "https://brasilapi.com.br/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}

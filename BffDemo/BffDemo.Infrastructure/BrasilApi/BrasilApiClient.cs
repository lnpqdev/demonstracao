using System.Net;
using System.Net.Http.Json;
using BffDemo.Application.Abstractions;
using BffDemo.Domain.Enderecos;
using Microsoft.Extensions.Logging;

namespace BffDemo.Infrastructure.BrasilApi;

/// <summary>
/// ADAPTER: implementa a port IEnderecoProvider (definida na Application)
/// usando a BrasilAPI via Typed HttpClient.
///
/// Responsabilidades típicas de Infrastructure:
///  1. Falar com o mundo externo (HTTP).
///  2. Traduzir o DTO EXTERNO -> entidade de DOMÍNIO.
///  3. Traduzir detalhes de transporte (404) em conceitos da aplicação (null).
///
/// É "internal": ninguém fora da Infrastructure precisa referenciá-lo
/// diretamente — a Application só conhece a interface.
/// </summary>
internal sealed class BrasilApiClient : IEnderecoProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrasilApiClient> _logger;

    public BrasilApiClient(HttpClient httpClient, ILogger<BrasilApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Endereco?> ObterPorCepAsync(
        string cep,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"api/cep/v2/{cep}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("CEP {Cep} não encontrado na BrasilAPI.", cep);
            return null;
        }

        response.EnsureSuccessStatusCode();

        var externo = await response.Content
            .ReadFromJsonAsync<EnderecoExternoResponse>(
                cancellationToken: cancellationToken);

        if (externo is null)
        {
            return null;
        }

        // Mapeia contrato EXTERNO -> entidade de DOMÍNIO.
        return new Endereco(
            cep: externo.Cep,
            logradouro: externo.Rua,
            bairro: externo.Bairro,
            cidade: externo.Cidade,
            uf: externo.Estado);
    }
}

using System.Text.Json.Serialization;

namespace BffDemo.Infrastructure.BrasilApi;

/// <summary>
/// DTO do contrato EXTERNO (JSON da BrasilAPI). Vive na Infrastructure
/// porque é um detalhe do adapter — o resto da aplicação não deve nem saber
/// que ele existe. Se a API de terceiros mudar, só este arquivo e o
/// mapeamento no BrasilApiClient são afetados.
/// </summary>
internal sealed record EnderecoExternoResponse
{
    [JsonPropertyName("cep")]
    public string Cep { get; init; } = string.Empty;

    [JsonPropertyName("state")]
    public string Estado { get; init; } = string.Empty;

    [JsonPropertyName("city")]
    public string Cidade { get; init; } = string.Empty;

    [JsonPropertyName("neighborhood")]
    public string Bairro { get; init; } = string.Empty;

    [JsonPropertyName("street")]
    public string Rua { get; init; } = string.Empty;
}

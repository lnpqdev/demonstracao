using BffDemo.Application.Abstractions;
using MediatR;

namespace BffDemo.Application.Ceps.ObterCep;

/// <summary>
/// Caso de uso (orquestração da aplicação). Depende apenas da PORT
/// (IEnderecoProvider) e do DOMÍNIO (Endereco) — nunca de HTTP/BrasilAPI.
///
/// Fluxo: normaliza o CEP -> pede o Endereço à port -> mapeia a entidade
/// de domínio para o DTO de saída (usando a regra de domínio Formatar()).
/// </summary>
public sealed class ObterCepHandler
    : IRequestHandler<ObterCepQuery, ObterCepResponse?>
{
    private readonly IEnderecoProvider _enderecoProvider;

    public ObterCepHandler(IEnderecoProvider enderecoProvider)
    {
        _enderecoProvider = enderecoProvider;
    }

    public async Task<ObterCepResponse?> Handle(
        ObterCepQuery request,
        CancellationToken cancellationToken)
    {
        var cepLimpo = new string(request.Cep.Where(char.IsDigit).ToArray());

        var endereco = await _enderecoProvider
            .ObterPorCepAsync(cepLimpo, cancellationToken);

        if (endereco is null)
        {
            return null; // A camada Api transforma isso em 404.
        }

        // Mapeia entidade de DOMÍNIO -> DTO de SAÍDA.
        // A regra de formatação vem do domínio (endereco.Formatar()).
        return new ObterCepResponse(
            Cep: endereco.Cep,
            Logradouro: endereco.Logradouro,
            Bairro: endereco.Bairro,
            Cidade: endereco.Cidade,
            Uf: endereco.Uf,
            EnderecoFormatado: endereco.Formatar());
    }
}

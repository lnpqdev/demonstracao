using MediatR;

namespace BffDemo.Application.Ceps.ObterCep;

/// <summary>
/// A mensagem do caso de uso (input). Vive na Application porque representa
/// uma intenção de negócio ("obter um CEP"), independente de HTTP.
/// </summary>
public sealed record ObterCepQuery(string Cep) : IRequest<ObterCepResponse?>;

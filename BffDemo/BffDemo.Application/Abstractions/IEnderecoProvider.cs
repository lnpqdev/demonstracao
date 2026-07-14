using BffDemo.Domain.Enderecos;

namespace BffDemo.Application.Abstractions;

/// <summary>
/// PORT (porta) da arquitetura Ports &amp; Adapters.
///
/// A camada Application DEFINE esta interface, mas NÃO a implementa. Ela
/// declara apenas o que precisa: "dado um CEP, me devolva um Endereço de
/// domínio". Quem sabe COMO obter (BrasilAPI, ViaCEP, banco, cache...) é a
/// Infrastructure, que fornece o "adapter".
///
/// Repare que o retorno é uma entidade de DOMÍNIO (Endereco), não um DTO
/// externo. Assim a Application nunca fica sabendo que existe HTTP ou
/// BrasilAPI — esse é o Princípio da Inversão de Dependência (o "D" do SOLID).
/// </summary>
public interface IEnderecoProvider
{
    Task<Endereco?> ObterPorCepAsync(string cep, CancellationToken cancellationToken);
}

namespace BffDemo.Application.Ceps.ObterCep;

/// <summary>
/// DTO de saída do caso de uso (contrato do BFF para o frontend).
/// Fica na Application: é o formato que o caso de uso entrega. A camada
/// Api apenas serializa isso como JSON, sem conhecer a regra por trás.
/// </summary>
public sealed record ObterCepResponse(
    string Cep,
    string Logradouro,
    string Bairro,
    string Cidade,
    string Uf,
    string EnderecoFormatado);

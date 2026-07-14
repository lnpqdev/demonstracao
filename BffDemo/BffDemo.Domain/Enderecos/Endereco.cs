namespace BffDemo.Domain.Enderecos;

/// <summary>
/// CAMADA DOMAIN — o núcleo da aplicação.
///
/// Esta entidade representa o conceito de negócio "Endereço" e NÃO conhece
/// nada externo: sem HTTP, sem MediatR, sem banco, sem framework web.
/// Só C# puro. É a camada mais interna da Clean Architecture; todas as
/// dependências apontam PARA cá, e ela não referencia nenhuma outra.
///
/// A regra de como um endereço é "formatado" mora aqui (comportamento de
/// domínio), e não espalhada em Handlers/Controllers.
/// </summary>
public sealed class Endereco
{
    public string Cep { get; }
    public string Logradouro { get; }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Uf { get; }

    public Endereco(
        string cep,
        string logradouro,
        string bairro,
        string cidade,
        string uf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }

    /// <summary>
    /// Regra de negócio de domínio: como exibir o endereço em uma linha.
    /// Fica no domínio para ser reutilizável e testável isoladamente.
    /// </summary>
    public string Formatar() =>
        $"{Logradouro}, {Bairro} - {Cidade}/{Uf}";
}

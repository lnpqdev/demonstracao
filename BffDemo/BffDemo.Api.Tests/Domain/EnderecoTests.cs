using BffDemo.Domain.Enderecos;
using Xunit;

namespace BffDemo.Api.Tests.Domain;

/// <summary>
/// Testes da entidade de DOMÍNIO. São os mais simples e rápidos possíveis:
/// nada de mocks, DI ou I/O — só a regra de negócio pura. É um benefício
/// direto da Clean Architecture: o domínio é 100% testável isoladamente.
/// </summary>
public sealed class EnderecoTests
{
    [Fact]
    public void Formatar_DeveMontarEnderecoEmUmaLinha()
    {
        var endereco = new Endereco(
            cep: "30110005",
            logradouro: "Avenida do Contorno",
            bairro: "Floresta",
            cidade: "Belo Horizonte",
            uf: "MG");

        Assert.Equal(
            "Avenida do Contorno, Floresta - Belo Horizonte/MG",
            endereco.Formatar());
    }
}

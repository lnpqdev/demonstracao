using BffDemo.Application.Abstractions;
using BffDemo.Application.Ceps.ObterCep;
using BffDemo.Domain.Enderecos;
using Moq;
using Xunit;

namespace BffDemo.Api.Tests.Features.Ceps;

/// <summary>
/// Testes de UNIDADE do caso de uso. Com Clean Architecture, o Handler
/// depende da PORT (IEnderecoProvider), então mockamos a port e devolvemos
/// uma entidade de DOMÍNIO (Endereco). Nenhum HTTP, nenhuma internet.
/// </summary>
public sealed class ObterCepHandlerTests
{
    private readonly Mock<IEnderecoProvider> _enderecoProviderMock = new();

    [Fact]
    public async Task Handle_QuandoCepExiste_DeveMapearParaContratoDoBff()
    {
        // Arrange
        var endereco = new Endereco(
            cep: "01310100",
            logradouro: "Avenida Paulista",
            bairro: "Bela Vista",
            cidade: "São Paulo",
            uf: "SP");

        _enderecoProviderMock
            .Setup(p => p.ObterPorCepAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(endereco);

        var handler = new ObterCepHandler(_enderecoProviderMock.Object);

        // Act
        var resultado = await handler.Handle(
            new ObterCepQuery("01310100"), CancellationToken.None);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal("Avenida Paulista", resultado!.Logradouro);
        Assert.Equal("SP", resultado.Uf);
        Assert.Equal(
            "Avenida Paulista, Bela Vista - São Paulo/SP",
            resultado.EnderecoFormatado);
    }

    [Fact]
    public async Task Handle_DeveNormalizarCepComHifen_AntesDeChamarProvider()
    {
        // Arrange
        _enderecoProviderMock
            .Setup(p => p.ObterPorCepAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Endereco("", "", "", "", ""));

        var handler = new ObterCepHandler(_enderecoProviderMock.Object);

        // Act
        await handler.Handle(new ObterCepQuery("01310-100"), CancellationToken.None);

        // Assert: a port foi chamada com o CEP JÁ normalizado (só dígitos).
        _enderecoProviderMock.Verify(
            p => p.ObterPorCepAsync("01310100", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoCepNaoExiste_DeveRetornarNull()
    {
        // Arrange
        _enderecoProviderMock
            .Setup(p => p.ObterPorCepAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Endereco?)null);

        var handler = new ObterCepHandler(_enderecoProviderMock.Object);

        // Act
        var resultado = await handler.Handle(
            new ObterCepQuery("00000000"), CancellationToken.None);

        // Assert
        Assert.Null(resultado);
    }
}

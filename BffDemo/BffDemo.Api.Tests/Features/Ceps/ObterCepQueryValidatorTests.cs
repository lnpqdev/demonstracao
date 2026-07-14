using BffDemo.Application.Ceps.ObterCep;
using Xunit;

namespace BffDemo.Api.Tests.Features.Ceps;

/// <summary>
/// Testes do Validator (FluentValidation). Rápidos e sem dependências:
/// só exercitam as regras de entrada. Usamos [Theory] para testar
/// vários casos com um só método.
/// </summary>
public sealed class ObterCepQueryValidatorTests
{
    private readonly ObterCepQueryValidator _validator = new();

    [Theory]
    [InlineData("01310100")]   // 8 dígitos
    [InlineData("01310-100")]  // com hífen também é válido
    public void Validate_CepValido_DevePassar(string cep)
    {
        var resultado = _validator.Validate(new ObterCepQuery(cep));
        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData("")]           // vazio
    [InlineData("123")]        // poucos dígitos
    [InlineData("012345678")]  // dígitos demais
    [InlineData("abcdefgh")]   // não numérico
    public void Validate_CepInvalido_DeveFalhar(string cep)
    {
        var resultado = _validator.Validate(new ObterCepQuery(cep));
        Assert.False(resultado.IsValid);
        Assert.NotEmpty(resultado.Errors);
    }
}

using FluentValidation;

namespace BffDemo.Application.Ceps.ObterCep;

/// <summary>
/// Validação de entrada do caso de uso. Vive junto do caso de uso na
/// Application. É descoberto pelo AddValidatorsFromAssembly e executado
/// pelo ValidationBehavior antes do Handler.
/// </summary>
public sealed class ObterCepQueryValidator : AbstractValidator<ObterCepQuery>
{
    public ObterCepQueryValidator()
    {
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("O CEP é obrigatório.")
            .Must(TerOitoDigitos)
            .WithMessage("O CEP deve conter exatamente 8 dígitos numéricos.");
    }

    private static bool TerOitoDigitos(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return false;
        }

        var apenasDigitos = new string(cep.Where(char.IsDigit).ToArray());
        return apenasDigitos.Length == 8;
    }
}

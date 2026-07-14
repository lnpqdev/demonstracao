using FluentValidation;
using MediatR;

namespace BffDemo.Application.Behaviors;

/// <summary>
/// Behavior transversal: executa os validators (FluentValidation) antes do
/// Handler. Se houver falhas, lança ValidationException e o Handler nem roda.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var falhas = _validators
            .Select(v => v.Validate(context))
            .SelectMany(resultado => resultado.Errors)
            .Where(erro => erro is not null)
            .ToList();

        if (falhas.Count != 0)
        {
            throw new ValidationException(falhas);
        }

        return await next();
    }
}

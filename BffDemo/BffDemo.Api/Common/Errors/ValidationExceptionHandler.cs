using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BffDemo.Api.Common.Errors;

/// <summary>
/// IExceptionHandler é a forma moderna (ASP.NET Core 8) de tratar exceções
/// de forma global, sem try/catch espalhado. Aqui capturamos a
/// ValidationException (lançada pelo ValidationBehavior) e a convertemos
/// em uma resposta HTTP 400 com ProblemDetails (formato padrão RFC 7807).
///
/// Registrado no Program.cs via AddExceptionHandler + UseExceptionHandler.
/// </summary>
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Só tratamos ValidationException aqui; o resto segue o pipeline padrão.
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var erros = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(erros)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais erros de validação ocorreram.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails, cancellationToken);

        return true; // exceção tratada; não propaga.
    }
}

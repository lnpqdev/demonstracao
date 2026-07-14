using BffDemo.Application.Ceps.ObterCep;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BffDemo.Api.Controllers;

/// <summary>
/// O Controller no BFF é DELIBERADAMENTE fino ("thin controller").
/// Ele só: (1) recebe o input HTTP, (2) monta a Query, (3) envia via
/// IMediator e (4) traduz o resultado em status HTTP. Nenhuma regra
/// de negócio mora aqui — tudo isso está no Handler.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class CepsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CepsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>GET /api/ceps/{cep} — ex.: /api/ceps/01310100</summary>
    [HttpGet("{cep}")]
    [ProducesResponseType(typeof(ObterCepResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorCep(
        string cep,
        CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(
            new ObterCepQuery(cep),
            cancellationToken);

        return resultado is null
            ? NotFound(new { mensagem = $"CEP {cep} não encontrado." })
            : Ok(resultado);
    }
}

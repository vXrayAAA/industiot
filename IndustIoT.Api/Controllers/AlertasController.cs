using IndustIoT.Api.Models.Enums;
using IndustIoT.Api.Services;
using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustIoT.Api.Controllers;

[ApiController]
[Route("api/alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _alertaService;

    public AlertasController(IAlertaService alertaService)
    {
        _alertaService = alertaService;
    }

    /// <summary>
    /// Lista alertas paginados, ordenados por severidade e data.
    /// Filtros opcionais: resolvido (bool) e severidade (Atencao, Alta, Critica).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AlertaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AlertaResponse>>> Listar(
        [FromQuery] bool? resolvido,
        [FromQuery] SeveridadeAlerta? severidade,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return Ok(await _alertaService.ListarAsync(resolvido, severidade, pageNumber, pageSize, ct));
    }

    /// <summary>
    /// Marca um alerta como resolvido. Exige autenticação (qualquer role).
    /// </summary>
    [HttpPatch("{id:long}/resolver")]
    [Authorize]
    [ProducesResponseType(typeof(AlertaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AlertaResponse>> Resolver(long id, CancellationToken ct)
    {
        return Ok(await _alertaService.ResolverAsync(id, ct));
    }
}

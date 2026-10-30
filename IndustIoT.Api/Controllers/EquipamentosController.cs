using IndustIoT.Api.Services;
using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndustIoT.Api.Controllers;

[ApiController]
[Route("api/equipamentos")]
public class EquipamentosController : ControllerBase
{
    private readonly IEquipamentoService _equipamentoService;
    private readonly IComandoService _comandoService;

    public EquipamentosController(IEquipamentoService equipamentoService, IComandoService comandoService)
    {
        _equipamentoService = equipamentoService;
        _comandoService = comandoService;
    }

    /// <summary>Cadastra um equipamento da planta (TAG única).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EquipamentoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EquipamentoResponse>> Criar(
        [FromBody] CriarEquipamentoRequest request, CancellationToken ct)
    {
        var equipamento = await _equipamentoService.CriarAsync(request, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = equipamento.Id }, equipamento);
    }

    /// <summary>Lista equipamentos paginados, com filtro opcional por setor.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EquipamentoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EquipamentoResponse>>> Listar(
        [FromQuery] string? setor,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return Ok(await _equipamentoService.ListarAsync(setor, pageNumber, pageSize, ct));
    }

    /// <summary>Detalha um equipamento.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EquipamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EquipamentoResponse>> ObterPorId(int id, CancellationToken ct)
    {
        return Ok(await _equipamentoService.ObterPorIdAsync(id, ct));
    }

    /// <summary>
    /// Histórico de consumo paginado do equipamento, com filtro opcional de período.
    /// </summary>
    [HttpGet("{id:int}/consumo")]
    [ProducesResponseType(typeof(PagedResult<ConsumoLeituraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ConsumoLeituraResponse>>> ObterConsumo(
        int id,
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return Ok(await _equipamentoService.ObterConsumoAsync(id, inicio, fim, pageNumber, pageSize, ct));
    }

    /// <summary>
    /// Endpoint crítico: desligamento remoto do equipamento. Exige JWT com role Gestor.
    /// </summary>
    [HttpPost("{id:int}/comando-desligamento")]
    [Authorize(Roles = "Gestor")]
    [ProducesResponseType(typeof(ComandoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ComandoResponse>> ComandoDesligamento(
        int id, [FromBody] ComandoDesligamentoRequest request, CancellationToken ct)
    {
        var usuario = User.Identity?.Name ?? "desconhecido";
        return Ok(await _comandoService.SolicitarDesligamentoAsync(id, request, usuario, ct));
    }
}

using IndustIoT.Api.Services;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.AspNetCore.Mvc;

namespace IndustIoT.Api.Controllers;

[ApiController]
[Route("api/leituras")]
public class LeiturasController : ControllerBase
{
    private readonly ILeituraService _leituraService;

    public LeiturasController(ILeituraService leituraService)
    {
        _leituraService = leituraService;
    }

    /// <summary>
    /// Recebe a leitura do gateway IoT. Se o consumo exceder o limite do
    /// equipamento, o alerta automático é gerado e retornado na resposta.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(LeituraRegistradaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeituraRegistradaResponse>> Registrar(
        [FromBody] RegistrarLeituraRequest request, CancellationToken ct)
    {
        var resultado = await _leituraService.RegistrarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }
}

using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Api.Services;

public interface IComandoService
{
    /// <summary>
    /// Solicita o desligamento remoto de um equipamento. Endpoint crítico:
    /// exige usuário autenticado, registrado para auditoria.
    /// </summary>
    Task<ComandoResponse> SolicitarDesligamentoAsync(
        int equipamentoId, ComandoDesligamentoRequest request, string usuario, CancellationToken ct = default);
}

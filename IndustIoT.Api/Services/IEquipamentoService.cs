using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Api.Services;

public interface IEquipamentoService
{
    Task<EquipamentoResponse> CriarAsync(CriarEquipamentoRequest request, CancellationToken ct = default);
    Task<EquipamentoResponse> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<EquipamentoResponse>> ListarAsync(string? setor, int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Histórico de consumo paginado de um equipamento, com filtro opcional de período.
    /// </summary>
    Task<PagedResult<ConsumoLeituraResponse>> ObterConsumoAsync(
        int id, DateTime? inicio, DateTime? fim, int pageNumber, int pageSize, CancellationToken ct = default);
}

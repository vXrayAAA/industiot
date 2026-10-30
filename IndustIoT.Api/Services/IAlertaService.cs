using IndustIoT.Api.Models.Enums;
using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Api.Services;

public interface IAlertaService
{
    Task<PagedResult<AlertaResponse>> ListarAsync(
        bool? resolvido, SeveridadeAlerta? severidade, int pageNumber, int pageSize, CancellationToken ct = default);

    Task<AlertaResponse> ResolverAsync(long id, CancellationToken ct = default);
}

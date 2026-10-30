using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Api.Services;

public interface ILeituraService
{
    /// <summary>
    /// Persiste a leitura do sensor e, se o consumo exceder o limite do
    /// equipamento, gera o alerta automático na mesma transação.
    /// </summary>
    Task<LeituraRegistradaResponse> RegistrarAsync(RegistrarLeituraRequest request, CancellationToken ct = default);
}

using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Api.Services;

public interface ITokenService
{
    /// <summary>
    /// Valida as credenciais e emite o JWT. Retorna null se inválidas
    /// (o controller responde 401 sem revelar qual campo errou).
    /// </summary>
    LoginResponse? Autenticar(LoginRequest request);
}

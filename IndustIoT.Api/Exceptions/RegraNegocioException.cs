namespace IndustIoT.Api.Exceptions;

/// <summary>
/// Violação de regra de negócio — mapeada para HTTP 422 pelo middleware global.
/// </summary>
public class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem) : base(mensagem) { }
}

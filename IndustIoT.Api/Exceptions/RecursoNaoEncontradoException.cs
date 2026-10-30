namespace IndustIoT.Api.Exceptions;

/// <summary>
/// Mapeada para HTTP 404 pelo middleware global de exceções.
/// </summary>
public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string mensagem) : base(mensagem) { }
}

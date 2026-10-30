using System.Text.Json;
using IndustIoT.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace IndustIoT.Api.Middlewares;

/// <summary>
/// Tratamento centralizado de exceções: nenhum controller precisa de try/catch.
/// Erros viram ProblemDetails (RFC 7807) com o status HTTP adequado.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RecursoNaoEncontradoException ex)
        {
            await EscreverProblemaAsync(context, StatusCodes.Status404NotFound,
                "Recurso não encontrado", ex.Message);
        }
        catch (RegraNegocioException ex)
        {
            await EscreverProblemaAsync(context, StatusCodes.Status422UnprocessableEntity,
                "Regra de negócio violada", ex.Message);
        }
        catch (Exception ex)
        {
            // Erro inesperado: loga o detalhe completo, mas não vaza stack trace ao cliente.
            _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}",
                context.Request.Method, context.Request.Path);

            await EscreverProblemaAsync(context, StatusCodes.Status500InternalServerError,
                "Erro interno", "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static async Task EscreverProblemaAsync(
        HttpContext context, int statusCode, string titulo, string detalhe)
    {
        var problema = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = detalhe,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problema, JsonOptions));
    }
}

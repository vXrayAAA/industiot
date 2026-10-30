using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Json.Schema;

namespace IndustIoT.AcceptanceTests.Support;

/// <summary>
/// Teste de contrato: valida o corpo da resposta contra um JSON Schema (draft 2020-12)
/// da pasta Schemas/. Os schemas usam additionalProperties=false nas respostas de sucesso:
/// qualquer campo novo, renomeado ou com tipo trocado quebra o contrato e falha o teste.
/// </summary>
public static class ValidadorContrato
{
    private static readonly ConcurrentDictionary<string, JsonSchema> Cache = new();

    static ValidadorContrato()
    {
        // Registra todos os schemas para que $ref entre arquivos (ex.: paged-alertas -> alerta) resolva.
        foreach (var arquivo in Directory.GetFiles(PastaSchemas, "*.schema.json"))
        {
            var schema = JsonSchema.FromFile(arquivo);
            SchemaRegistry.Global.Register(schema);
            Cache[Path.GetFileName(arquivo).Replace(".schema.json", "")] = schema;
        }
    }

    private static string PastaSchemas => Path.Combine(AppContext.BaseDirectory, "Schemas");

    public static IReadOnlyList<string> Validar(string nomeContrato, JsonNode? corpo)
    {
        if (!Cache.TryGetValue(nomeContrato, out var schema))
            throw new ArgumentException($"Contrato '{nomeContrato}' não existe em {PastaSchemas}.");

        var resultado = schema.Evaluate(corpo, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true
        });

        if (resultado.IsValid)
            return Array.Empty<string>();

        return (resultado.Details ?? Array.Empty<EvaluationResults>())
            .Where(d => d.HasErrors)
            .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation} ({d.EvaluationPath}): {e.Key} — {e.Value}"))
            .Distinct()
            .ToList();
    }
}

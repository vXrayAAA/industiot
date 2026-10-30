using System.Globalization;
using System.Text.Json.Nodes;
using IndustIoT.AcceptanceTests.Support;
using Reqnroll;

namespace IndustIoT.AcceptanceTests.StepDefinitions;

/// <summary>
/// Steps genéricos de validação da resposta HTTP: status code, corpo JSON e contrato (JSON Schema).
/// Reutilizados por todas as funcionalidades.
/// </summary>
[Binding]
public class RespostaSteps
{
    private readonly ContextoApi _ctx;

    public RespostaSteps(ContextoApi ctx) => _ctx = ctx;

    [Then("o status da resposta deve ser {int}")]
    public void EntaoStatus(int esperado)
    {
        Assert.True((int)_ctx.Status == esperado,
            $"Status esperado {esperado}, recebido {(int)_ctx.Status}. Corpo: {_ctx.CorpoBruto}");
    }

    [Then("o corpo da resposta deve respeitar o contrato {string}")]
    public void EntaoContrato(string contrato)
    {
        Assert.NotNull(_ctx.Corpo);
        var erros = ValidadorContrato.Validar(contrato, _ctx.Corpo);
        Assert.True(erros.Count == 0,
            $"Resposta viola o contrato '{contrato}':\n - {string.Join("\n - ", erros)}\nCorpo: {_ctx.CorpoBruto}");
        Console.WriteLine($"[CONTRATO] '{contrato}.schema.json' OK");
    }

    [Then("o campo {string} da resposta deve ser {string}")]
    public void EntaoCampoIgual(string caminho, string esperado)
    {
        var no = _ctx.Campo(caminho);
        Assert.True(no is not null, $"Campo '{caminho}' ausente. Corpo: {_ctx.CorpoBruto}");
        Assert.True(ValoresIguais(no!, esperado),
            $"Campo '{caminho}': esperado '{esperado}', recebido '{Texto(no)}'.");
    }

    [Then("o campo {string} da resposta deve conter {string}")]
    public void EntaoCampoContem(string caminho, string trecho)
    {
        var texto = Texto(_ctx.Campo(caminho));
        Assert.Contains(trecho, texto);
    }

    [Then("o campo {string} da resposta deve ser nulo")]
    public void EntaoCampoNulo(string caminho)
    {
        Assert.Null(_ctx.Campo(caminho));
    }

    [Then("a resposta não deve conter o campo {string}")]
    public void EntaoSemCampo(string caminho)
    {
        Assert.Null(_ctx.Campo(caminho));
    }

    [Then("a lista {string} deve ter {int} elementos")]
    public void EntaoListaTamanho(string caminho, int tamanho)
    {
        var lista = Assert.IsType<JsonArray>(_ctx.Campo(caminho));
        Assert.Equal(tamanho, lista.Count);
    }

    [Then("todos os itens devem ter o campo {string} igual a {string}")]
    public void EntaoTodosItens(string campo, string esperado)
    {
        var itens = Assert.IsType<JsonArray>(_ctx.Campo("items"));
        Assert.NotEmpty(itens);
        foreach (var item in itens)
        {
            var valor = item![campo];
            Assert.True(valor is not null && ValoresIguais(valor, esperado),
                $"Item com '{campo}' = '{Texto(valor)}' (esperado '{esperado}'): {item.ToJsonString()}");
        }
    }

    [Then("a resposta deve conter erro de validação para o campo {string}")]
    public void EntaoErroValidacao(string campo)
    {
        var erros = Assert.IsType<JsonObject>(_ctx.Campo("errors"));
        var chaves = erros.Select(e => e.Key).ToList();
        Assert.True(chaves.Any(c => c.Equals(campo, StringComparison.OrdinalIgnoreCase)),
            $"Esperado erro para '{campo}'. Erros retornados: {string.Join(", ", chaves)}");
    }

    internal static bool ValoresIguais(JsonNode no, string esperado)
    {
        if (no is JsonValue valor)
        {
            if (valor.TryGetValue<string>(out var s)) return s == esperado;
            if (valor.TryGetValue<bool>(out var b)) return b.ToString().Equals(esperado, StringComparison.OrdinalIgnoreCase);
            if (decimal.TryParse(no.ToJsonString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var n) &&
                decimal.TryParse(esperado, NumberStyles.Number, CultureInfo.InvariantCulture, out var e))
                return n == e;
        }
        return no.ToJsonString() == esperado;
    }

    internal static string Texto(JsonNode? no) => no switch
    {
        null => "(nulo)",
        JsonValue v when v.TryGetValue<string>(out var s) => s,
        _ => no.ToJsonString()
    };
}

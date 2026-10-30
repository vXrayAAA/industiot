using System.Text.Json.Nodes;
using IndustIoT.AcceptanceTests.Support;
using Reqnroll;

namespace IndustIoT.AcceptanceTests.StepDefinitions;

[Binding]
public class LeituraEAlertaSteps
{
    private readonly ContextoApi _ctx;

    public LeituraEAlertaSteps(ContextoApi ctx) => _ctx = ctx;

    private static object Leitura(string tag, decimal consumo, DateTime? quando = null, decimal? temperatura = null) => new
    {
        equipamentoTag = tag,
        consumoKwh = consumo,
        temperaturaCelsius = temperatura ?? 65.4m,
        vibracaoMmPorSegundo = 2.1m,
        correnteAmperes = 87.5m,
        dataHoraLeitura = quando ?? DateTime.UtcNow.AddSeconds(-5)
    };

    // ---------- Leituras ----------

    [When("o sensor envia uma leitura de {decimal} kWh para o equipamento")]
    public async Task QuandoLeitura(decimal consumo)
    {
        await _ctx.PostAsync("api/leituras", Leitura(_ctx.EquipamentoTag, consumo));
        if (_ctx.Campo("alerta.id") is { } id)
            _ctx.AlertaId = id.GetValue<long>();
    }

    [When("o sensor envia uma leitura de {decimal} kWh para a TAG {string}")]
    public async Task QuandoLeituraTag(decimal consumo, string tag)
        => await _ctx.PostAsync("api/leituras", Leitura(tag, consumo));

    [When("o sensor envia uma leitura inválida com {}")]
    public async Task QuandoLeituraInvalida(string problema)
    {
        var corpo = problema switch
        {
            "consumo negativo" => Leitura(_ctx.EquipamentoTag, -1m),
            "data no futuro" => Leitura(_ctx.EquipamentoTag, 10m, DateTime.UtcNow.AddDays(1)),
            "temperatura de 900 graus" => Leitura(_ctx.EquipamentoTag, 10m, temperatura: 900m),
            "TAG em minúsculas" => Leitura(_ctx.EquipamentoTag.ToLowerInvariant(), 10m),
            _ => throw new ArgumentException($"Problema não mapeado: {problema}")
        };
        await _ctx.PostAsync("api/leituras", corpo);
    }

    [Given("que o sensor enviou as leituras de consumo:")]
    public async Task DadoLeituras(Table tabela)
    {
        foreach (var linha in tabela.Rows)
        {
            var quando = DateTime.UtcNow.AddMinutes(-int.Parse(linha["minutosAtras"]));
            var resposta = await _ctx.PostAsync("api/leituras",
                Leitura(_ctx.EquipamentoTag, decimal.Parse(linha["consumoKwh"], System.Globalization.CultureInfo.InvariantCulture), quando));
            Assert.Equal(201, (int)resposta.StatusCode);
        }
    }

    [Given("que o sensor gerou um alerta de consumo para o equipamento")]
    public async Task DadoAlertaGerado()
    {
        // 80 kWh com limite de 50 kWh = 60% acima -> Critica
        var resposta = await _ctx.PostAsync("api/leituras", Leitura(_ctx.EquipamentoTag, 80m));
        Assert.Equal(201, (int)resposta.StatusCode);
        _ctx.AlertaId = _ctx.Campo("alerta.id")!.GetValue<long>();
    }

    // ---------- Alertas ----------

    [When("eu listo os alertas não resolvidos com severidade {string}")]
    public async Task QuandoListoAlertas(string severidade)
        => await _ctx.GetAsync($"api/alertas?resolvido=false&severidade={severidade}&pageSize=100");

    [Then("a listagem de alertas deve conter o alerta gerado para o equipamento")]
    public void EntaoListagemContemAlerta()
    {
        var itens = Assert.IsType<JsonArray>(_ctx.Campo("items"));
        var alerta = itens.FirstOrDefault(i => i!["id"]!.GetValue<long>() == _ctx.AlertaId);
        Assert.True(alerta is not null, $"Alerta {_ctx.AlertaId} não está na listagem.");
        Assert.Equal(_ctx.EquipamentoTag, alerta!["equipamentoTag"]!.GetValue<string>());
    }

    [Given("que o alerta gerado já foi resolvido")]
    public async Task DadoAlertaResolvido()
    {
        var resposta = await _ctx.PatchAsync($"api/alertas/{_ctx.AlertaId}/resolver", autenticado: true);
        Assert.Equal(200, (int)resposta.StatusCode);
    }

    [When("eu resolvo o alerta gerado")]
    public async Task QuandoResolvo() => await _ctx.PatchAsync($"api/alertas/{_ctx.AlertaId}/resolver", autenticado: true);

    [When("eu resolvo o alerta gerado sem token")]
    public async Task QuandoResolvoSemToken() => await _ctx.PatchAsync($"api/alertas/{_ctx.AlertaId}/resolver", autenticado: false);

    [When("eu resolvo o alerta de id {long}")]
    public async Task QuandoResolvoId(long id) => await _ctx.PatchAsync($"api/alertas/{id}/resolver", autenticado: true);

    [Then("o alerta não deve mais aparecer entre os não resolvidos")]
    public async Task EntaoAlertaSumiuDosPendentes()
    {
        await _ctx.GetAsync("api/alertas?resolvido=false&pageSize=100");
        var itens = Assert.IsType<JsonArray>(_ctx.Campo("items"));
        Assert.DoesNotContain(itens, i => i!["id"]!.GetValue<long>() == _ctx.AlertaId);
    }
}

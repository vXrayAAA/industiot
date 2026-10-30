using System.Text.Json.Nodes;
using IndustIoT.AcceptanceTests.Support;
using Reqnroll;

namespace IndustIoT.AcceptanceTests.StepDefinitions;

[Binding]
public class EquipamentoSteps
{
    private readonly ContextoApi _ctx;

    public EquipamentoSteps(ContextoApi ctx) => _ctx = ctx;

    private object NovoEquipamento(string tag, string nome, decimal potencia, decimal limite, string? setor = null) => new
    {
        tag,
        nome,
        fabricante = "WEG",
        modelo = "W22 IR3",
        setor = setor ?? "Utilidades",
        linhaProducao = "Linha 2",
        potenciaNominalWatts = potencia,
        limiteConsumoKwh = limite
    };

    /// <summary>Cria o equipamento via API (pré-condição) e guarda id/TAG no contexto do cenário.</summary>
    internal async Task CriarEquipamentoAsync(decimal limite, string? setor = null)
    {
        var tag = DadosTeste.NovaTag();
        var resposta = await _ctx.PostAsync("api/equipamentos", NovoEquipamento(tag, "Compressor de ar", 55000, limite, setor));
        Assert.True((int)resposta.StatusCode == 201, $"Pré-condição: cadastro do equipamento falhou ({(int)resposta.StatusCode}): {_ctx.CorpoBruto}");
        _ctx.EquipamentoId = _ctx.Campo("id")!.GetValue<int>();
        _ctx.EquipamentoTag = _ctx.Campo("tag")!.GetValue<string>();
    }

    [Given("que existe um equipamento cadastrado com limite de consumo de {decimal} kWh")]
    public Task DadoEquipamento(decimal limite) => CriarEquipamentoAsync(limite);

    [Given("que existem {int} equipamentos cadastrados no mesmo setor")]
    public async Task DadoEquipamentosNoSetor(int quantidade)
    {
        for (var i = 0; i < quantidade; i++)
            await CriarEquipamentoAsync(30, _ctx.SetorDoCenario);
    }

    [When("eu cadastro um equipamento com nova TAG, potência de {decimal} W e limite de {decimal} kWh")]
    public async Task QuandoCadastroValido(decimal potencia, decimal limite)
    {
        _ctx.EquipamentoTag = DadosTeste.NovaTag();
        await _ctx.PostAsync("api/equipamentos", NovoEquipamento(_ctx.EquipamentoTag, "Torno CNC", potencia, limite));
        if ((int)_ctx.Status == 201)
            _ctx.EquipamentoId = _ctx.Campo("id")!.GetValue<int>();
    }

    [When("eu cadastro outro equipamento com a mesma TAG")]
    public async Task QuandoCadastroDuplicado()
        => await _ctx.PostAsync("api/equipamentos", NovoEquipamento(_ctx.EquipamentoTag, "Outro equipamento", 1000, 10));

    [When("eu cadastro um equipamento com TAG {string}, nome {string}, potência de {decimal} W e limite de {decimal} kWh")]
    public async Task QuandoCadastroComDados(string tag, string nome, decimal potencia, decimal limite)
        => await _ctx.PostAsync("api/equipamentos", NovoEquipamento(tag, nome, potencia, limite));

    [When("eu listo os equipamentos desse setor com página {int} e tamanho {int}")]
    public async Task QuandoListoPorSetor(int pagina, int tamanho)
        => await _ctx.GetAsync($"api/equipamentos?setor={Uri.EscapeDataString(_ctx.SetorDoCenario)}&pageNumber={pagina}&pageSize={tamanho}");

    [When("eu consulto esse equipamento")]
    public async Task QuandoConsultoEsse() => await _ctx.GetAsync($"api/equipamentos/{_ctx.EquipamentoId}");

    [When("eu consulto o equipamento de id {int}")]
    public async Task QuandoConsultoId(int id) => await _ctx.GetAsync($"api/equipamentos/{id}");

    [When("eu consulto o histórico de consumo desse equipamento")]
    public async Task QuandoConsultoConsumo() => await _ctx.GetAsync($"api/equipamentos/{_ctx.EquipamentoId}/consumo");

    [When("eu consulto o histórico de consumo do equipamento de id {int}")]
    public async Task QuandoConsultoConsumoId(int id) => await _ctx.GetAsync($"api/equipamentos/{id}/consumo");

    [Then("o cabeçalho Location deve apontar para o equipamento criado")]
    public void EntaoLocation()
    {
        var location = _ctx.Resposta!.Headers.Location;
        Assert.NotNull(location);
        Assert.EndsWith($"/api/equipamentos/{_ctx.EquipamentoId}", location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Then("o campo {string} da resposta deve ser a TAG do equipamento")]
    public void EntaoCampoTag(string campo) => Assert.Equal(_ctx.EquipamentoTag, RespostaSteps.Texto(_ctx.Campo(campo)));

    [Then("os itens devem estar ordenados da leitura mais recente para a mais antiga")]
    public void EntaoOrdenado()
    {
        var datas = Assert.IsType<JsonArray>(_ctx.Campo("items"))
            .Select(i => DateTime.Parse(i!["dataHoraLeitura"]!.GetValue<string>()))
            .ToList();
        Assert.Equal(datas.OrderByDescending(d => d).ToList(), datas);
    }

    [Then("o equipamento deve ficar com status {string}")]
    public async Task EntaoStatusEquipamento(string status)
    {
        await _ctx.GetAsync($"api/equipamentos/{_ctx.EquipamentoId}");
        Assert.Equal(status, RespostaSteps.Texto(_ctx.Campo("status")));
    }
}

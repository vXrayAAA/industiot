using IndustIoT.AcceptanceTests.Support;
using Reqnroll;

namespace IndustIoT.AcceptanceTests.StepDefinitions;

[Binding]
public class ComandoSteps
{
    private readonly ContextoApi _ctx;

    public ComandoSteps(ContextoApi ctx) => _ctx = ctx;

    private Task Desligar(int id, string? motivo, bool autenticado)
        => _ctx.PostAsync($"api/equipamentos/{id}/comando-desligamento", new { motivo }, autenticado);

    [When("eu envio o comando de desligamento com o motivo {string}")]
    public Task QuandoDesligo(string motivo) => Desligar(_ctx.EquipamentoId, motivo, autenticado: true);

    [When("eu envio o comando de desligamento sem token")]
    public Task QuandoDesligoSemToken() => Desligar(_ctx.EquipamentoId, "Sem token", autenticado: false);

    [When("eu envio o comando de desligamento para o equipamento de id {int}")]
    public Task QuandoDesligoId(int id) => Desligar(id, "Equipamento inexistente", autenticado: true);

    [When("eu envio o comando de desligamento com um motivo de {int} caracteres")]
    public Task QuandoDesligoMotivoLongo(int tamanho) => Desligar(_ctx.EquipamentoId, new string('x', tamanho), autenticado: true);

    [Given("que o equipamento já foi desligado remotamente")]
    public async Task DadoJaDesligado()
    {
        await Desligar(_ctx.EquipamentoId, "Desligamento inicial", autenticado: true);
        Assert.Equal(200, (int)_ctx.Status);
    }
}

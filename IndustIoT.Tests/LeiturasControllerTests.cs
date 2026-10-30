using System.Net.Http.Json;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;

namespace IndustIoT.Tests;

public class LeiturasControllerTests : IClassFixture<IndustIoTApiFactory>
{
    private readonly HttpClient _client;

    public LeiturasControllerTests(IndustIoTApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task CriarEquipamentoAsync(string tag, decimal limiteKwh)
    {
        var equipamento = new CriarEquipamentoRequest
        {
            Tag = tag,
            Nome = "Compressor de ar — Linha 2",
            Fabricante = "Atlas Copco",
            Modelo = "GA 55",
            Setor = "Utilidades",
            LinhaProducao = "Linha 2",
            PotenciaNominalWatts = 55000,
            LimiteConsumoKwh = limiteKwh
        };

        var response = await _client.PostAsJsonAsync("/api/equipamentos", equipamento);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Post_LeituraDentroDoLimite_ReturnsSuccessStatusCode()
    {
        // Arrange
        await CriarEquipamentoAsync("CMP-10", limiteKwh: 50m);
        var leitura = new RegistrarLeituraRequest
        {
            EquipamentoTag = "CMP-10",
            ConsumoKwh = 30m,
            DataHoraLeitura = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/leituras", leitura);

        // Assert
        response.EnsureSuccessStatusCode();

        var corpo = await response.Content.ReadFromJsonAsync<LeituraRegistradaResponse>();
        Assert.NotNull(corpo);
        Assert.False(corpo.AlertaGerado);
    }

    [Fact]
    public async Task Post_LeituraAcimaDoLimite_GeraAlertaComSeveridadeCorreta()
    {
        // Arrange — 80 kWh com limite de 50 kWh = 60% de excesso -> Critica
        await CriarEquipamentoAsync("CMP-11", limiteKwh: 50m);
        var leitura = new RegistrarLeituraRequest
        {
            EquipamentoTag = "CMP-11",
            ConsumoKwh = 80m,
            DataHoraLeitura = DateTime.UtcNow
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/leituras", leitura);

        // Assert
        response.EnsureSuccessStatusCode();

        var corpo = await response.Content.ReadFromJsonAsync<LeituraRegistradaResponse>();
        Assert.NotNull(corpo);
        Assert.True(corpo.AlertaGerado);
        Assert.NotNull(corpo.Alerta);
        Assert.Equal("Critica", corpo.Alerta!.Severidade);
        Assert.Equal(60.0m, corpo.Alerta.ExcessoPercentual);
    }
}

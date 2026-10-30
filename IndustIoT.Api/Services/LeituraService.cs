using IndustIoT.Api.Data;
using IndustIoT.Api.Exceptions;
using IndustIoT.Api.Models;
using IndustIoT.Api.Models.Enums;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.Services;

public class LeituraService : ILeituraService
{
    private readonly AppDbContext _db;

    public LeituraService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LeituraRegistradaResponse> RegistrarAsync(RegistrarLeituraRequest request, CancellationToken ct = default)
    {
        var equipamento = await _db.Equipamentos
            .FirstOrDefaultAsync(e => e.Tag == request.EquipamentoTag, ct)
            ?? throw new RecursoNaoEncontradoException(
                $"Equipamento com TAG '{request.EquipamentoTag}' não encontrado.");

        var leitura = new LeituraSensor
        {
            EquipamentoId = equipamento.Id,
            ConsumoKwh = request.ConsumoKwh,
            TemperaturaCelsius = request.TemperaturaCelsius,
            VibracaoMmPorSegundo = request.VibracaoMmPorSegundo,
            CorrenteAmperes = request.CorrenteAmperes,
            DataHoraLeitura = request.DataHoraLeitura
        };
        _db.LeiturasSensor.Add(leitura);

        Alerta? alerta = null;
        if (request.ConsumoKwh > equipamento.LimiteConsumoKwh)
        {
            var severidade = CalcularSeveridade(request.ConsumoKwh, equipamento.LimiteConsumoKwh);
            var excesso = CalcularExcessoPercentual(request.ConsumoKwh, equipamento.LimiteConsumoKwh);

            alerta = new Alerta
            {
                Equipamento = equipamento,
                LeituraSensor = leitura,
                ConsumoRegistradoKwh = request.ConsumoKwh,
                LimiteConfiguradoKwh = equipamento.LimiteConsumoKwh,
                Severidade = severidade,
                Mensagem = $"Consumo de {request.ConsumoKwh:F2} kWh excedeu o limite de " +
                           $"{equipamento.LimiteConsumoKwh:F2} kWh ({excesso:F1}% acima) " +
                           $"no equipamento {equipamento.Tag} — {equipamento.Nome}."
            };
            _db.Alertas.Add(alerta);
        }

        // Leitura e alerta são salvos juntos: ou ambos entram, ou nenhum.
        await _db.SaveChangesAsync(ct);

        return new LeituraRegistradaResponse
        {
            LeituraId = leitura.Id,
            EquipamentoTag = equipamento.Tag,
            ConsumoKwh = leitura.ConsumoKwh,
            DataHoraLeitura = leitura.DataHoraLeitura,
            AlertaGerado = alerta is not null,
            Alerta = alerta is null ? null : new AlertaResponse
            {
                Id = alerta.Id,
                EquipamentoTag = equipamento.Tag,
                Severidade = alerta.Severidade.ToString(),
                ConsumoRegistradoKwh = alerta.ConsumoRegistradoKwh,
                LimiteConfiguradoKwh = alerta.LimiteConfiguradoKwh,
                ExcessoPercentual = CalcularExcessoPercentual(alerta.ConsumoRegistradoKwh, alerta.LimiteConfiguradoKwh),
                Mensagem = alerta.Mensagem,
                DataHoraCriacao = alerta.DataHoraCriacao,
                Resolvido = alerta.Resolvido
            }
        };
    }

    private static decimal CalcularExcessoPercentual(decimal consumoKwh, decimal limiteKwh)
        => limiteKwh <= 0 ? 0 : Math.Round((consumoKwh - limiteKwh) / limiteKwh * 100, 1);

    /// <summary>
    /// Classifica a gravidade do estouro de consumo. Só é chamado quando
    /// consumoKwh > limiteKwh (garantido pelo chamador).
    /// </summary>
    private static SeveridadeAlerta CalcularSeveridade(decimal consumoKwh, decimal limiteKwh)
    {
        // Cortes calibrados para evitar fadiga de alerta: estouros pequenos são
        // comuns em picos de operação (Atencao); acima de 50% do limite indica
        // desperdício grave ou defeito e exige reação imediata (Critica).
        var excessoPercentual = CalcularExcessoPercentual(consumoKwh, limiteKwh);

        return excessoPercentual switch
        {
            <= 20 => SeveridadeAlerta.Atencao,
            <= 50 => SeveridadeAlerta.Alta,
            _ => SeveridadeAlerta.Critica
        };
    }
}

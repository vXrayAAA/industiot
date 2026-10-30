using IndustIoT.Api.Data;
using IndustIoT.Api.Exceptions;
using IndustIoT.Api.Models;
using IndustIoT.Api.Models.Enums;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.Services;

public class ComandoService : IComandoService
{
    private readonly AppDbContext _db;

    public ComandoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ComandoResponse> SolicitarDesligamentoAsync(
        int equipamentoId, ComandoDesligamentoRequest request, string usuario, CancellationToken ct = default)
    {
        var equipamento = await _db.Equipamentos
            .FirstOrDefaultAsync(e => e.Id == equipamentoId, ct)
            ?? throw new RecursoNaoEncontradoException($"Equipamento {equipamentoId} não encontrado.");

        if (equipamento.Status == StatusEquipamento.Desligado)
            throw new RegraNegocioException($"Equipamento {equipamento.Tag} já está desligado.");

        if (equipamento.Status == StatusEquipamento.EmManutencao)
            throw new RegraNegocioException(
                $"Equipamento {equipamento.Tag} está em manutenção — comando remoto bloqueado por segurança.");

        // Simula o ciclo gateway IoT: comando despachado e confirmado de forma síncrona.
        // Em produção, o status ficaria Pendente até o ACK do gateway.
        var comando = new ComandoAtuacao
        {
            EquipamentoId = equipamento.Id,
            Tipo = TipoComando.Desligamento,
            Status = StatusComando.Confirmado,
            SolicitadoPor = usuario,
            Motivo = request.Motivo,
            DataHoraExecucao = DateTime.UtcNow
        };

        equipamento.Status = StatusEquipamento.Desligado;
        _db.ComandosAtuacao.Add(comando);
        await _db.SaveChangesAsync(ct);

        return new ComandoResponse
        {
            Id = comando.Id,
            EquipamentoTag = equipamento.Tag,
            Tipo = comando.Tipo.ToString(),
            Status = comando.Status.ToString(),
            SolicitadoPor = comando.SolicitadoPor,
            Motivo = comando.Motivo,
            DataHoraSolicitacao = comando.DataHoraSolicitacao,
            DataHoraExecucao = comando.DataHoraExecucao
        };
    }
}

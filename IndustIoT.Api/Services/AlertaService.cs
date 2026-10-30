using IndustIoT.Api.Data;
using IndustIoT.Api.Exceptions;
using IndustIoT.Api.Models;
using IndustIoT.Api.Models.Enums;
using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.Services;

public class AlertaService : IAlertaService
{
    private readonly AppDbContext _db;

    public AlertaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<AlertaResponse>> ListarAsync(
        bool? resolvido, SeveridadeAlerta? severidade, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Alertas.AsNoTracking();

        if (resolvido.HasValue)
            query = query.Where(a => a.Resolvido == resolvido.Value);
        if (severidade.HasValue)
            query = query.Where(a => a.Severidade == severidade.Value);

        return await query
            .OrderByDescending(a => a.Severidade)
            .ThenByDescending(a => a.DataHoraCriacao)
            .Select(a => new AlertaResponse
            {
                Id = a.Id,
                EquipamentoTag = a.Equipamento.Tag,
                Severidade = a.Severidade.ToString(),
                ConsumoRegistradoKwh = a.ConsumoRegistradoKwh,
                LimiteConfiguradoKwh = a.LimiteConfiguradoKwh,
                ExcessoPercentual = a.LimiteConfiguradoKwh <= 0 ? 0 :
                    Math.Round((a.ConsumoRegistradoKwh - a.LimiteConfiguradoKwh) / a.LimiteConfiguradoKwh * 100, 1),
                Mensagem = a.Mensagem,
                DataHoraCriacao = a.DataHoraCriacao,
                Resolvido = a.Resolvido
            })
            .ToPagedResultAsync(pageNumber, pageSize, ct);
    }

    public async Task<AlertaResponse> ResolverAsync(long id, CancellationToken ct = default)
    {
        var alerta = await _db.Alertas
            .Include(a => a.Equipamento)
            .FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new RecursoNaoEncontradoException($"Alerta {id} não encontrado.");

        if (alerta.Resolvido)
            throw new RegraNegocioException($"Alerta {id} já foi resolvido em {alerta.DataHoraResolucao:dd/MM/yyyy HH:mm}.");

        alerta.Resolvido = true;
        alerta.DataHoraResolucao = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new AlertaResponse
        {
            Id = alerta.Id,
            EquipamentoTag = alerta.Equipamento.Tag,
            Severidade = alerta.Severidade.ToString(),
            ConsumoRegistradoKwh = alerta.ConsumoRegistradoKwh,
            LimiteConfiguradoKwh = alerta.LimiteConfiguradoKwh,
            ExcessoPercentual = alerta.LimiteConfiguradoKwh <= 0 ? 0 :
                Math.Round((alerta.ConsumoRegistradoKwh - alerta.LimiteConfiguradoKwh) / alerta.LimiteConfiguradoKwh * 100, 1),
            Mensagem = alerta.Mensagem,
            DataHoraCriacao = alerta.DataHoraCriacao,
            Resolvido = alerta.Resolvido
        };
    }
}

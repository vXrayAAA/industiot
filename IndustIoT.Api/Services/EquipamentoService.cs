using IndustIoT.Api.Data;
using IndustIoT.Api.Exceptions;
using IndustIoT.Api.Models;
using IndustIoT.Api.ViewModels.Common;
using IndustIoT.Api.ViewModels.Requests;
using IndustIoT.Api.ViewModels.Responses;
using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.Services;

public class EquipamentoService : IEquipamentoService
{
    private readonly AppDbContext _db;

    public EquipamentoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EquipamentoResponse> CriarAsync(CriarEquipamentoRequest request, CancellationToken ct = default)
    {
        var tagJaExiste = await _db.Equipamentos.AnyAsync(e => e.Tag == request.Tag, ct);
        if (tagJaExiste)
            throw new RegraNegocioException($"Já existe um equipamento com a TAG '{request.Tag}'.");

        var equipamento = new Equipamento
        {
            Tag = request.Tag.ToUpperInvariant(),
            Nome = request.Nome,
            Fabricante = request.Fabricante,
            Modelo = request.Modelo,
            Setor = request.Setor,
            LinhaProducao = request.LinhaProducao,
            PotenciaNominalWatts = request.PotenciaNominalWatts,
            LimiteConsumoKwh = request.LimiteConsumoKwh
        };

        _db.Equipamentos.Add(equipamento);
        await _db.SaveChangesAsync(ct);

        return MapToResponse(equipamento);
    }

    public async Task<EquipamentoResponse> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var equipamento = await _db.Equipamentos.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new RecursoNaoEncontradoException($"Equipamento {id} não encontrado.");

        return MapToResponse(equipamento);
    }

    public async Task<PagedResult<EquipamentoResponse>> ListarAsync(
        string? setor, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Equipamentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(setor))
            query = query.Where(e => e.Setor == setor);

        return await query
            .OrderBy(e => e.Tag)
            .Select(e => new EquipamentoResponse
            {
                Id = e.Id,
                Tag = e.Tag,
                Nome = e.Nome,
                Fabricante = e.Fabricante,
                Modelo = e.Modelo,
                Setor = e.Setor,
                LinhaProducao = e.LinhaProducao,
                PotenciaNominalWatts = e.PotenciaNominalWatts,
                LimiteConsumoKwh = e.LimiteConsumoKwh,
                Status = e.Status.ToString(),
                DataCadastro = e.DataCadastro
            })
            .ToPagedResultAsync(pageNumber, pageSize, ct);
    }

    public async Task<PagedResult<ConsumoLeituraResponse>> ObterConsumoAsync(
        int id, DateTime? inicio, DateTime? fim, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var equipamentoExiste = await _db.Equipamentos.AnyAsync(e => e.Id == id, ct);
        if (!equipamentoExiste)
            throw new RecursoNaoEncontradoException($"Equipamento {id} não encontrado.");

        var query = _db.LeiturasSensor.AsNoTracking()
            .Where(l => l.EquipamentoId == id);

        if (inicio.HasValue)
            query = query.Where(l => l.DataHoraLeitura >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(l => l.DataHoraLeitura <= fim.Value);

        // Ordenação casa com o índice (EquipamentoId, DataHoraLeitura DESC):
        // o banco resolve a página direto no índice, sem sort extra.
        return await query
            .OrderByDescending(l => l.DataHoraLeitura)
            .Select(l => new ConsumoLeituraResponse
            {
                Id = l.Id,
                ConsumoKwh = l.ConsumoKwh,
                TemperaturaCelsius = l.TemperaturaCelsius,
                VibracaoMmPorSegundo = l.VibracaoMmPorSegundo,
                CorrenteAmperes = l.CorrenteAmperes,
                DataHoraLeitura = l.DataHoraLeitura
            })
            .ToPagedResultAsync(pageNumber, pageSize, ct);
    }

    private static EquipamentoResponse MapToResponse(Equipamento e) => new()
    {
        Id = e.Id,
        Tag = e.Tag,
        Nome = e.Nome,
        Fabricante = e.Fabricante,
        Modelo = e.Modelo,
        Setor = e.Setor,
        LinhaProducao = e.LinhaProducao,
        PotenciaNominalWatts = e.PotenciaNominalWatts,
        LimiteConsumoKwh = e.LimiteConsumoKwh,
        Status = e.Status.ToString(),
        DataCadastro = e.DataCadastro
    };
}

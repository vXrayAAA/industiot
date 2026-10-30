using IndustIoT.Api.Models.Enums;

namespace IndustIoT.Api.Models;

public class ComandoAtuacao
{
    public long Id { get; set; }
    public int EquipamentoId { get; set; }
    public TipoComando Tipo { get; set; }
    public StatusComando Status { get; set; } = StatusComando.Pendente;

    /// <summary>
    /// Usuário autenticado (claim do JWT) que solicitou o comando — trilha de auditoria.
    /// </summary>
    public string SolicitadoPor { get; set; } = string.Empty;

    public string? Motivo { get; set; }
    public DateTime DataHoraSolicitacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataHoraExecucao { get; set; }

    public Equipamento Equipamento { get; set; } = null!;
}

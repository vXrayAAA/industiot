namespace IndustIoT.Api.ViewModels.Responses;

public class ComandoResponse
{
    public long Id { get; set; }
    public string EquipamentoTag { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SolicitadoPor { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public DateTime DataHoraSolicitacao { get; set; }
    public DateTime? DataHoraExecucao { get; set; }
}

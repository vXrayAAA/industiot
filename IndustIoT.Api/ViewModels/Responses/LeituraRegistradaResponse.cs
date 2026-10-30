namespace IndustIoT.Api.ViewModels.Responses;

public class LeituraRegistradaResponse
{
    public long LeituraId { get; set; }
    public string EquipamentoTag { get; set; } = string.Empty;
    public decimal ConsumoKwh { get; set; }
    public DateTime DataHoraLeitura { get; set; }

    /// <summary>
    /// Indica se esta leitura disparou um alerta automático de consumo.
    /// </summary>
    public bool AlertaGerado { get; set; }

    public AlertaResponse? Alerta { get; set; }
}

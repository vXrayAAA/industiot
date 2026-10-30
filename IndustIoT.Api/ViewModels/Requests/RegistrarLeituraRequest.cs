namespace IndustIoT.Api.ViewModels.Requests;

/// <summary>
/// Payload enviado pelo gateway IoT. O sensor se identifica pela TAG da máquina.
/// </summary>
public class RegistrarLeituraRequest
{
    public string EquipamentoTag { get; set; } = string.Empty;
    public decimal ConsumoKwh { get; set; }
    public decimal? TemperaturaCelsius { get; set; }
    public decimal? VibracaoMmPorSegundo { get; set; }
    public decimal? CorrenteAmperes { get; set; }
    public DateTime DataHoraLeitura { get; set; }
}

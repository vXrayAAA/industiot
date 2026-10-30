namespace IndustIoT.Api.Models;

public class LeituraSensor
{
    public long Id { get; set; }
    public int EquipamentoId { get; set; }

    /// <summary>
    /// Grandeza central do monitoramento ESG: consumo no intervalo da leitura.
    /// </summary>
    public decimal ConsumoKwh { get; set; }

    // Grandezas de condição do equipamento (monitoramento preditivo IndústIoT).
    // Nem todo sensor reporta todas — por isso são opcionais.
    public decimal? TemperaturaCelsius { get; set; }
    public decimal? VibracaoMmPorSegundo { get; set; }
    public decimal? CorrenteAmperes { get; set; }

    public DateTime DataHoraLeitura { get; set; }
    public DateTime DataHoraRecebimento { get; set; } = DateTime.UtcNow;

    public Equipamento Equipamento { get; set; } = null!;
}

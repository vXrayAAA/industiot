namespace IndustIoT.Api.ViewModels.Responses;

public class ConsumoLeituraResponse
{
    public long Id { get; set; }
    public decimal ConsumoKwh { get; set; }
    public decimal? TemperaturaCelsius { get; set; }
    public decimal? VibracaoMmPorSegundo { get; set; }
    public decimal? CorrenteAmperes { get; set; }
    public DateTime DataHoraLeitura { get; set; }
}

namespace IndustIoT.Api.ViewModels.Requests;

public class CriarEquipamentoRequest
{
    public string Tag { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string LinhaProducao { get; set; } = string.Empty;
    public decimal PotenciaNominalWatts { get; set; }
    public decimal LimiteConsumoKwh { get; set; }
}

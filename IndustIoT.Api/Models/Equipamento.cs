using IndustIoT.Api.Models.Enums;

namespace IndustIoT.Api.Models;

public class Equipamento
{
    public int Id { get; set; }

    /// <summary>
    /// Identificador de planta no padrão IndústIoT (ex.: "CMP-02", "TRN-01").
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Setor { get; set; } = string.Empty;
    public string LinhaProducao { get; set; } = string.Empty;
    public decimal PotenciaNominalWatts { get; set; }

    /// <summary>
    /// Consumo máximo tolerado por leitura (kWh). Acima disso, um alerta é gerado.
    /// </summary>
    public decimal LimiteConsumoKwh { get; set; }

    public StatusEquipamento Status { get; set; } = StatusEquipamento.Ligado;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<LeituraSensor> Leituras { get; set; } = new List<LeituraSensor>();
    public ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
    public ICollection<ComandoAtuacao> Comandos { get; set; } = new List<ComandoAtuacao>();
}

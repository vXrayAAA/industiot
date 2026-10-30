using IndustIoT.Api.Models.Enums;

namespace IndustIoT.Api.Models;

public class Alerta
{
    public long Id { get; set; }
    public int EquipamentoId { get; set; }
    public long LeituraSensorId { get; set; }

    // Snapshot dos valores no momento do disparo: o limite do equipamento
    // pode ser alterado depois, mas o histórico do alerta precisa permanecer fiel.
    public decimal ConsumoRegistradoKwh { get; set; }
    public decimal LimiteConfiguradoKwh { get; set; }

    public SeveridadeAlerta Severidade { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataHoraCriacao { get; set; } = DateTime.UtcNow;
    public bool Resolvido { get; set; }
    public DateTime? DataHoraResolucao { get; set; }

    public Equipamento Equipamento { get; set; } = null!;
    public LeituraSensor LeituraSensor { get; set; } = null!;
}

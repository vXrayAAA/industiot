namespace IndustIoT.Api.ViewModels.Responses;

public class AlertaResponse
{
    public long Id { get; set; }
    public string EquipamentoTag { get; set; } = string.Empty;
    public string Severidade { get; set; } = string.Empty;
    public decimal ConsumoRegistradoKwh { get; set; }
    public decimal LimiteConfiguradoKwh { get; set; }

    /// <summary>
    /// Quanto o consumo excedeu o limite, em % (ex.: 35,2 = 35,2% acima).
    /// </summary>
    public decimal ExcessoPercentual { get; set; }

    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataHoraCriacao { get; set; }
    public bool Resolvido { get; set; }
}

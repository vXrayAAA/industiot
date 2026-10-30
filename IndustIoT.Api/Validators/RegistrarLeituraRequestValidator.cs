using FluentValidation;
using IndustIoT.Api.ViewModels.Requests;

namespace IndustIoT.Api.Validators;

public class RegistrarLeituraRequestValidator : AbstractValidator<RegistrarLeituraRequest>
{
    public RegistrarLeituraRequestValidator()
    {
        RuleFor(r => r.EquipamentoTag)
            .NotEmpty().WithMessage("A TAG do equipamento é obrigatória.")
            .Matches(@"^[A-Z]{2,4}-\d{2,3}$")
            .WithMessage("TAG fora do padrão de planta (ex.: CMP-02, TRN-01).");

        RuleFor(r => r.ConsumoKwh)
            .GreaterThanOrEqualTo(0).WithMessage("O consumo não pode ser negativo.")
            .LessThan(100_000).WithMessage("Consumo acima do plausível para uma leitura — verifique o sensor.");

        RuleFor(r => r.TemperaturaCelsius)
            .InclusiveBetween(-50, 500).When(r => r.TemperaturaCelsius.HasValue)
            .WithMessage("Temperatura fora da faixa plausível (-50 a 500 °C).");

        RuleFor(r => r.VibracaoMmPorSegundo)
            .GreaterThanOrEqualTo(0).When(r => r.VibracaoMmPorSegundo.HasValue)
            .WithMessage("A vibração não pode ser negativa.");

        RuleFor(r => r.CorrenteAmperes)
            .GreaterThanOrEqualTo(0).When(r => r.CorrenteAmperes.HasValue)
            .WithMessage("A corrente não pode ser negativa.");

        RuleFor(r => r.DataHoraLeitura)
            .NotEmpty().WithMessage("A data/hora da leitura é obrigatória.")
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("A leitura não pode estar no futuro (tolerância de 5 min para clock skew).");
    }
}

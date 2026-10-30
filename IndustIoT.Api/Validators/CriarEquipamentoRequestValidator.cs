using FluentValidation;
using IndustIoT.Api.ViewModels.Requests;

namespace IndustIoT.Api.Validators;

public class CriarEquipamentoRequestValidator : AbstractValidator<CriarEquipamentoRequest>
{
    public CriarEquipamentoRequestValidator()
    {
        RuleFor(r => r.Tag)
            .NotEmpty().WithMessage("A TAG é obrigatória.")
            .Matches(@"^[A-Za-z]{2,4}-\d{2,3}$")
            .WithMessage("TAG fora do padrão de planta (ex.: CMP-02, TRN-01).");

        RuleFor(r => r.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(150);

        RuleFor(r => r.Setor)
            .NotEmpty().WithMessage("O setor é obrigatório.")
            .MaximumLength(100);

        RuleFor(r => r.Fabricante).MaximumLength(100);
        RuleFor(r => r.Modelo).MaximumLength(100);
        RuleFor(r => r.LinhaProducao).MaximumLength(50);

        RuleFor(r => r.PotenciaNominalWatts)
            .GreaterThan(0).WithMessage("A potência nominal deve ser maior que zero.");

        RuleFor(r => r.LimiteConsumoKwh)
            .GreaterThan(0).WithMessage("O limite de consumo deve ser maior que zero.");
    }
}

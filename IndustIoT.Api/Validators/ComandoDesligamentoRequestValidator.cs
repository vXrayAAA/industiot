using FluentValidation;
using IndustIoT.Api.ViewModels.Requests;

namespace IndustIoT.Api.Validators;

public class ComandoDesligamentoRequestValidator : AbstractValidator<ComandoDesligamentoRequest>
{
    public ComandoDesligamentoRequestValidator()
    {
        RuleFor(r => r.Motivo)
            .MaximumLength(500).WithMessage("O motivo deve ter no máximo 500 caracteres.");
    }
}

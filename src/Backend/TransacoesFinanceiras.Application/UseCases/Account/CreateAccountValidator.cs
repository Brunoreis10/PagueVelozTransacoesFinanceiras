using FluentValidation;
using TransacoesFinanceiras.Application.Commands;

namespace TransacoesFinanceiras.Application.UseCases.Account
{
    public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountValidator()
        {
            RuleFor(x => x.Dto.ClientId)
                .NotEmpty().WithMessage("ClientId é obrigatório")
                .MaximumLength(100).WithMessage("ClientId não pode exceder 100 caracteres");

            RuleFor(x => x.Dto.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Saldo inicial não pode ser negativo");

            RuleFor(x => x.Dto.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Limite de crédito não pode ser negativo");
        }
    }
}

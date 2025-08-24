using FluentValidation;
using TaskMarketplace.Contracts.Tasks;

namespace TaskMarketplace.WebApi.Validators.Tasks
{
    public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Название задачи обязательно")
                .MinimumLength(3).WithMessage("Название задачи должно содержать минимум 3 символа")
                .MaximumLength(100).WithMessage("Название задачи не должно превышать 100 символов");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание задачи обязательно")
                .MaximumLength(500).WithMessage("Описание задачи не должно превышать 500 символов");

            RuleFor(x => x.Reward)
                .InclusiveBetween(1, 1000).WithMessage("Награда должна быть в диапазоне от 1 до 1000");

            RuleFor(x => x.TakenByUserId)
                .GreaterThan(0).When(x => x.TakenByUserId.HasValue)
                .WithMessage("ID пользователя должен быть положительным числом");
        }
    }
}
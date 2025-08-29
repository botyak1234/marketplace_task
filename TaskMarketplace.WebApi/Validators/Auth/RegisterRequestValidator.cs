using FluentValidation;
using TaskMarketplace.Contracts.Auth;

namespace TaskMarketplace.WebApi.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Имя пользователя обязательно")
                .MinimumLength(3).WithMessage("Имя пользователя должно содержать минимум 3 символа")
                .MaximumLength(50).WithMessage("Имя пользователя не должно превышать 50 символов")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("Имя пользователя может содержать только буквы, цифры и нижнее подчеркивание");

            RuleFor(x => x.HashPassword)
                .NotEmpty().WithMessage("Пароль обязателен")
                .MinimumLength(4).WithMessage("Пароль должен содержать минимум 4 символов");
        }
    }
}
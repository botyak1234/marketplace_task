using FluentValidation;
using TaskMarketplace.Contracts.Tasks;

namespace TaskMarketplace.WebApi.Validators.Tasks
{
    public class ReviewRequestValidator : AbstractValidator<ReviewRequest>
    {
        public ReviewRequestValidator()
        {
            RuleFor(x => x.StatusByAdmin)
                .IsInEnum().WithMessage("Статус должен быть 'Approved' или 'Rejected'");
        }
    }
}
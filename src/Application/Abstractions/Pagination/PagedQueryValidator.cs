using FluentValidation;

namespace Application.Abstractions.Pagination;

public abstract class PagedQueryValidator<TQuery> : AbstractValidator<TQuery>
    where TQuery : IPagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(q=> q.Search)
            .MinimumLength(3)
            .When( q=> !string.IsNullOrWhiteSpace(q.Search));

    }
}

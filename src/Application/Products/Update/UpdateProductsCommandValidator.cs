using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.Products.Update;

public class UpdateProductCommandValidator :AbstractValidator<UpdateProductCommand>
{
    // Matches: "0", "1250", "0,00", "149,50", "1.250,00", "0,00 kr.", "149,50 kr"
    private static readonly Regex LegacyDanishPriceRegex = new (
        @"^(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d{2})?(?:\s?kr\.)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(100) );

    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);
        RuleFor(c => c.Sku).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(c => c.Name).NotEmpty().MinimumLength(5).MaximumLength(1000);
        RuleFor(c => c.Price).NotEmpty().MaximumLength(50).Matches(LegacyDanishPriceRegex);
    }
}
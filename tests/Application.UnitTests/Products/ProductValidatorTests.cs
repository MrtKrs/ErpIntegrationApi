using Application.Products.Get;
using Application.Products.Update;
using Domain.Products;
using FluentValidation.TestHelper;

namespace  Application.UnitTests.Products;

public sealed class ProductValidatorTests
{
    private readonly GetProductsQueryValidator _getValidator = new();
    private readonly UpdateProductsCommandValidator _updateValidator = new();

    [Fact]
    public void GetValidator_Should_HaveError_WhenPageIsZeroOrNegative()
    {
        var query = new GetProductsQuery(Page: 0, PageSize: 10, Search: null, SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Page);
    }

    [Fact]
    public void GetValidator_Should_HaveError_WhenSearchIsTooShort()
    {
        var query = new GetProductsQuery(Page: 1, PageSize: 10, Search: "ab", SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.Search);
    }

    [Fact]
    public void GetValidator_Should_Pass_WhenQueryIsValid()
    {
        var query = new GetProductsQuery(Page: 1, PageSize: 10, Search: "Valid", SortColumn: null, SortOrder: null);

        var result = _getValidator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void UpdateValidator_Should_HaveError_WhenIdIsInvalid(int id)
    {
        var command = new UpdateProductCommand(Id: id, Sku: "Product-0", Name: "Product Zero", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor( c => c.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateValidator_Should_HaveError_WhenSkuIsEmpty(string sku)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: sku, Name: "Empty Sku", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor( c => c.Sku);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateValidator_Should_HaveError_WhenNameIsEmpty(string name)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "EmptyName", Name: name, Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);
        
        result.ShouldHaveValidationErrorFor( c => c.Name);
    }
    [Fact]
    public void UpdateValidator_Should_HaveError_WhenSkuIsNull()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: null!, Name: "Valid Name", Price: "1,00 kr.");

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Sku);
    }

    [Theory]
    [InlineData(3)]//TODO: Raise this to some business requirement
    [InlineData(50)]
    public void UpdateValidator_Should_NotHaveError_WhenSkuValidLength(int length)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: new string('a', length), Name: "Too long Sku", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);
        
        result.ShouldNotHaveValidationErrorFor(c=>c.Sku);
    }

    [Fact] 
    public void UpdateValidator_Should_HaveError_WhenSkuTooShort()
        {
        var command = new UpdateProductCommand(Id: 4, Sku: new string('a', 2), Name: "Too long Sku", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c=>c.Sku);
    }

    [Fact]
    public void UpdateValidator_Should_HaveError_WhenSkuTooLong()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: new string('a', 51), Name: "Too long Sku", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c=>c.Sku);
    }

    [Theory]
    [InlineData(5)]//TODO: Raise this to some business requirement
    [InlineData(1000)]
    public void UpdateValidator_Should_NotHaveError_WhenNameValidLength(int length)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "EmptyName", Name: new string('a', length), Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);
        
        result.ShouldNotHaveValidationErrorFor(c=>c.Name);
    }

    [Fact]
    public void UpdateValidator_Should_HaveError_WhenNameTooShort()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "TooLongName", Name: new string('a', 4), Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c=>c.Name);
    }

    [Fact]
    public void UpdateValidator_Should_HaveError_WhenNameTooLong()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "TooLongName", Name: new string('a', 1001), Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c=>c.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateValidator_Should_HaveError_WhenPriceIsEmpty(string price)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "SKU-1", Name: "Valid Name", Price: price);

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }
    [Fact]
    public void UpdateValidator_Should_HaveError_WhenPriceTooLong()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "SKU-1", Name: "Valid Name", Price: new string('1', 51));

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Theory]
    [InlineData("ABC")]         // No numeric value at all
    [InlineData("-10 kr.")]     // Negative price
    [InlineData("0,00 kr")]    
    public void UpdateValidator_Should_HaveError_WhenPriceFormatIsInvalid(string invalidPrice)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "SKU-1", Name: "Valid Name", Price: invalidPrice);

        var result = _updateValidator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Price);
    }

    [Theory]
    [InlineData("1,23 kr.")]
    [InlineData("19,00")]
    [InlineData("1.999,00 kr.")]
    [InlineData("0")]
    public void UpdateValidator_Should_NotHaveError_WhenPriceIsValid(string validPrice)
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "SKU-1", Name: "Valid Name", Price: validPrice);

        var result = _updateValidator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Price);
    }

    [Fact]
    public void UpdateValidator_Should_NotHaveAnyErrors_WhenCommandIsValid()
    {
        var command = new UpdateProductCommand(Id: 4, Sku: "ValidProduct", Name: "Valid Product", Price: "1,00 kr.");
        
        var result = _updateValidator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
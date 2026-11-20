using System;
using Bogus;
using IntegrationTests.Common.Database;

namespace IntegrationTests.Common.Fakers;

public static class OrderFaker
{
    private const string DocumentIdPrefix = "ORD-";
    private const string CustomerNumberPrefix = "DEB-";
    private const string InvoicePrefix = "FAK-";
    private const string ReferencePrefix = "REF-";
    private const int DefaultDocumentType = 3;
    private const int RandomDigitsLength = 6;
    private const int MinCustomerId = 1000;
    private const int MaxCustomerId = 9999;
    private const decimal MinNetAmount = 100m;
    private const decimal MaxNetAmount = 5000m;
    private const decimal DanishVatRate = 0.25m;
    private const int PastDaysRange = 30;

    public static Faker<TestOrder> SeedOrder {get;} = new Faker<TestOrder>()
    .CustomInstantiator(f => 
    {
        decimal net = Math.Round(f.Random.Decimal(MinNetAmount, MaxNetAmount), 2);
        decimal vat = Math.Round(net * DanishVatRate, 2);

        return new TestOrder(
            Id: 0,
            DocumentId: $"{DocumentIdPrefix}{f.Random.Number(100000, 999999)}", 
            DocumentType: DefaultDocumentType,
            OrderDate: f.Date.Recent(PastDaysRange).Date,
            NetAmount: FakerDefaults.FormatAmount(net), 
            VatAmount: FakerDefaults.FormatAmount(vat), 
            TotalAmount: FakerDefaults.FormatAmount(net + vat), 
            CustomerId: f.Random.Number(MinCustomerId, MaxCustomerId), 
            CustomerNumber: $"{CustomerNumberPrefix}{f.Random.Number(100000, 999999)}", 
            CustomerName: f.Company.CompanyName(), 
            Notes: f.Lorem.Sentence(), 
            SalespersonName: f.Name.FullName(), 
            InvoiceNumber: $"{InvoicePrefix}{f.Random.Number(100000,999999)}", 
            ReferencesDocumentNumber: $"{ReferencePrefix}{f.Random.AlphaNumeric(RandomDigitsLength).ToUpperInvariant()}"
        );
    });
}

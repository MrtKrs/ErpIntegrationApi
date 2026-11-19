using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class TenantContext : ITenantContext
{
    public string? DatabaseName {get; private set;}

    public void SetDatabase(string databaseName) => DatabaseName = databaseName;
}

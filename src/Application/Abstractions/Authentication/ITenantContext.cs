namespace Application.Abstractions.Authentication;

public interface ITenantContext
{
    string? DatabaseName {get;}
    void SetDatabase(string databaseName);
}

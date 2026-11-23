namespace Application.Abstractions.Authentication;
public sealed record ApiKeyValidationResult(
    int ClientId,
    string ClientName,
    string DatabaseName,
    IReadOnlyCollection<string> Permissions);
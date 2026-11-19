namespace Application.Abstractions.Authentication;
public sealed record ApiKeyValidationResult(
    string ClientId,
    string ClientName,
    string DatabaseName,
    IReadOnlyCollection<string> Permissions);
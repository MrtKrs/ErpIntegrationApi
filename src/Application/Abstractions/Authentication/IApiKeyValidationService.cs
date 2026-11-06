using System;

namespace Application.Abstractions.Authentication;

public interface IApiKeyValidationService
{
    Task<(bool IsValild, string[] Permissions)> ValidateKeysAsync(string apiKey, CancellationToken cancellationToken);
}

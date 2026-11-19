using System;

namespace Application.Abstractions.Authentication;

public interface IApiKeyValidationService
{
    Task<ApiKeyValidationResult?> ValidateAsync (string apiKey, CancellationToken cancellationToken);
}

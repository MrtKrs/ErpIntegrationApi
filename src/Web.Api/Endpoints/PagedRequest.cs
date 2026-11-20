namespace Web.Api.Endpoints;

public sealed record PagedRequest(
    string? Search = null,
    int Page = 1,
    int PageSize = 10);

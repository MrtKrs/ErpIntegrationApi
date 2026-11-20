using System;

namespace Application.Abstractions.Pagination;

public interface IPagedQuery
{
    string? Search{get;}
    int Page{get;}
    int PageSize{get;}
}

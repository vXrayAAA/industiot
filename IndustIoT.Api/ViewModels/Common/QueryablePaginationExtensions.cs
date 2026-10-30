using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.ViewModels.Common;

public static class QueryablePaginationExtensions
{
    public const int TamanhoMaximoPagina = 100;

    /// <summary>
    /// Materializa apenas a página pedida: COUNT + OFFSET/FETCH no banco,
    /// nunca a tabela inteira em memória.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, TamanhoMaximoPagina);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

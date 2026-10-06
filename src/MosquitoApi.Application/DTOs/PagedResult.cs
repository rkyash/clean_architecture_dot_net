namespace MosquitoApi.Application.DTOs;

/// <summary>
/// Generic wrapper for paginated results
/// </summary>
/// <typeparam name="T">Type of items in the result set</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; init; }

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Number of the first item on current page
    /// </summary>
    public int FirstItemOnPage { get; init; }

    /// <summary>
    /// Number of the last item on current page
    /// </summary>
    public int LastItemOnPage { get; init; }

    /// <summary>
    /// Creates a new PagedResult instance
    /// </summary>
    /// <param name="items">Items for the current page</param>
    /// <param name="pageNumber">Current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="totalCount">Total number of items</param>
    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalCount)
    {
        var itemsList = items.ToList();
        
        Items = itemsList.AsReadOnly();
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        HasPreviousPage = pageNumber > 1;
        HasNextPage = pageNumber < TotalPages;
        
        if (itemsList.Count > 0)
        {
            FirstItemOnPage = (pageNumber - 1) * pageSize + 1;
            LastItemOnPage = FirstItemOnPage + itemsList.Count - 1;
        }
        else
        {
            FirstItemOnPage = 0;
            LastItemOnPage = 0;
        }
    }

    /// <summary>
    /// Creates an empty PagedResult
    /// </summary>
    /// <param name="pageNumber">Current page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Empty paged result</returns>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20)
    {
        return new PagedResult<T>([], pageNumber, pageSize, 0);
    }

    /// <summary>
    /// Creates a PagedResult from a full list of items by taking the appropriate page
    /// </summary>
    /// <param name="allItems">All items</param>
    /// <param name="pageNumber">Page number to extract</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paged result for the specified page</returns>
    public static PagedResult<T> FromList(IEnumerable<T> allItems, int pageNumber, int pageSize)
    {
        var itemsList = allItems.ToList();
        var totalCount = itemsList.Count;
        var pagedItems = itemsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>(pagedItems, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Maps the items in this PagedResult to a different type
    /// </summary>
    /// <typeparam name="TResult">Target type</typeparam>
    /// <param name="mapper">Function to map each item</param>
    /// <returns>New PagedResult with mapped items</returns>
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        var mappedItems = Items.Select(mapper).ToList();
        return new PagedResult<TResult>(mappedItems, PageNumber, PageSize, TotalCount);
    }

    /// <summary>
    /// Gets pagination metadata as a summary string
    /// </summary>
    /// <returns>Human-readable pagination summary</returns>
    public string GetSummary()
    {
        if (TotalCount == 0)
            return "No items found";

        if (TotalCount <= PageSize)
            return $"Showing all {TotalCount} items";

        return $"Showing {FirstItemOnPage}-{LastItemOnPage} of {TotalCount} items (Page {PageNumber} of {TotalPages})";
    }
}
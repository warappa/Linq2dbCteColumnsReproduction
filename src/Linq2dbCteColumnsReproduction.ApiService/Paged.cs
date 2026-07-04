public record Paged<TData>(int Page, int PageSize, int TotalCount, TData[] Data);

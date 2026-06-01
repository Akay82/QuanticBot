namespace QuanticApi.Business.Contracts;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 20)
{
    public int SafePageNumber => Math.Max(1, PageNumber);
    public int SafePageSize => Math.Clamp(PageSize, 1, 100);
}

namespace GymSystem.BLL.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    // عدد الصفحات = الإجمالي ÷ حجم الصفحة مقرّب لفوق
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items.ToList();
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    // بيانات الترقيم جاهزة للـ Partial
    public PaginationInfo Info => new(Page, TotalPages, HasPrevious, HasNext);
}
using SharedDataModels.DTOs;

namespace SharedDataModels.CustomClasses
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
    public class TRow
    {
        public int Index { get; set; }
        public required URLDTO URL { get; set; }
        public bool isNew { get; set; }
    }

}

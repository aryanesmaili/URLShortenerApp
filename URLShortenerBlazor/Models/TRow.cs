using URLShortener.Application.DTOs;

namespace SharedDataModels.CustomClasses
{
    public class TRow
    {
        public int Index { get; set; }
        public required URLDTO URL { get; set; }
        public bool IsNew { get; set; }
    }
}

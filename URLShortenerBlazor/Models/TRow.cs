using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace SharedDataModels.CustomClasses
{
    public class TRow
    {
        public int Index { get; set; }
        public required URLDTO URL { get; set; }
        public bool IsNew { get; set; }
    }
}

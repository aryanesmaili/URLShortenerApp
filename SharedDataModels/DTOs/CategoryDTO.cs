using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDataModels.DTOs
{
    public class CategoryDTO
    {
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }

        public int UserID { get; set; }
        public List<URLDTO>? URLs { get; set; }
    }
}

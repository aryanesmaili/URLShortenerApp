using URLShortener.Application.DTOs.EntityDTOs.Category;
using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserDTO
{
    public int ID { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public DateTime CreatedAt { get; set; }


    public List<URLDTO>? URLs { get; set; }
    public List<CategoryDTO>? Categories { get; set; } // owned categories
}

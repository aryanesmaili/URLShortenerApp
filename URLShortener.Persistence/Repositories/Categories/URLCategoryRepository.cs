using URLShortener.Application.Features.Categories.Interfaces.Repositories;
using URLShortener.Domain.Entities.URLCategory;
using URLShortenerAPI.Data;

namespace URLShortener.Persistence.Repositories.Categories;

public sealed class URLCategoryRepository(AppDbContext context) : GenericRepository<URLCategoryModel>(context), IURLCategoryRepository
{
}

using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Domain.Entities.URLCategory;

namespace URLShortener.Application.Features.Categories.Interfaces.Repositories;

public interface IURLCategoryRepository : IGenericRepository<URLCategoryModel>
{
}

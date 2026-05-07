using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Domain.Entities.URL;

namespace URLShortener.Application.Features.URLs.Interfaces.Repositories;

public interface IURLRepository : IGenericRepository<URLModel>, IGetPaged<URLModel>
{

}

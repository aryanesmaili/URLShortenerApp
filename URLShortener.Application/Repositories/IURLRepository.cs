using URLShortener.Domain.Entities.URL;

namespace URLShortener.Application.Repositories;

public interface IURLRepository : IGenericRepository<URLModel>, IGetPaged<URLModel>
{

}

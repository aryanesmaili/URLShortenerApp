using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class URLService : IURLService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        public URLService(AppDbContext context, IMapper mapper, IAuthService authService)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
        }

        public async Task<URLDTO> AddURL(URLCreateDTO url)
        {
            UserModel user = (await _context.Users.FindAsync(url.UserID)) ?? throw new NotFoundException($"User {url.UserID} Does not Exist");
            URLModel newRecord = _mapper.Map<URLModel>(url);

            _context.URLs.Add(newRecord);
            await _context.SaveChangesAsync();
            return URLModelToDTO(newRecord);
        }

        public async Task<URLDTO> GetURL(int urlID)
        {
            URLModel url = await _context.URLs.FindAsync(urlID) ?? throw new NotFoundException($"URL {urlID} Does not Exist");
            return URLModelToDTO(url);
        }

        public async Task ToggleActivation(int URLID, string reqUsername)
        {
            URLModel url = await _authService.AuthorizeURLAccessAsync(URLID, reqUsername);
            url.IsActive = !url.IsActive;
            return;
        }

        public string ShortURLGenerator(string LongURL)
        {
            throw new NotImplementedException();
        }

        public URLDTO URLModelToDTO(URLModel url)
        {
            return _mapper.Map<URLDTO>(url);
        }
    }
}

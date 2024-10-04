using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using System.Text;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Services
{
    internal class URLService : IURLService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IShortenerService _shortenerService;

        public URLService(AppDbContext context, IMapper mapper, IAuthService authService, IShortenerService shortenerService)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _shortenerService = shortenerService;
        }
        /// <summary>
        /// Adding a new URL to database after checking if this operation is authorized to happen.
        /// </summary>
        /// <param name="url">the object containing info about the new URL to be added.</param>
        /// <param name="username">string containing the username requesting this operation.</param>
        /// <returns> a <see cref="URLDTO"/> object representing the operation</returns>
        public async Task<URLDTO> AddURL(URLCreateDTO url, string username)
        {
            // Authorize user access.
            UserModel user = await _authService.AuthorizeUserAccessAsync(url.UserID, username);

            // creating a new object to be later added to DB.
            URLModel newRecord = _mapper.Map<URLModel>(url);
            // after making sure this operation is authorized we assign the user.
            newRecord.User = user;
            // Generating the short URL
            newRecord.ShortCode = await ShortURLGenerator(url.LongURL);

            await _context.URLs.AddAsync(newRecord);
            await _context.SaveChangesAsync();

            return URLModelToDTO(newRecord);
        }
        /// <summary>
        /// Get a URL's Info from database.
        /// </summary>
        /// <param name="urlID">ID of the URL to fetch.</param>
        /// <returns>a <see cref="URLDTO"/>object containing info about the record.</returns>
        /// <exception cref="NotFoundException">thrown if no object with this ID exists.</exception>
        public async Task<URLDTO> GetURL(int urlID)
        {
            URLModel url = await _context.URLs
                .Include(x => x.URLAnalyticsID)
                .Include(x => x.CategoryID)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == urlID) 
                ?? throw new NotFoundException($"URL {urlID} Does not Exist");
            return URLModelToDTO(url);
        }
        /// <summary>
        /// Activate or Deactivates the url.
        /// </summary>
        /// <param name="URLID"> ID of the URL to be toggled.</param>
        /// <param name="reqUsername">username requesting this operation.</param>
        /// <returns></returns>
        public async Task ToggleActivation(int URLID, string reqUsername)
        {
            URLModel url = await _authService.AuthorizeURLAccessAsync(URLID, reqUsername);
            url.IsActive = !url.IsActive;
            return;
        }
        /// <summary>
        /// Takes A long URL and converts it to shortened version.
        /// </summary>
        /// <param name="longURL">URL to be converted.</param>
        /// <returns>a string containing the shortened version of the URL.</returns>
        public async Task<string> ShortURLGenerator(string longURL)
        {
            string shortenedURL = _shortenerService.HashURL(longURL);

            // checking if we have any collisions
            if (await _context.URLs.FirstOrDefaultAsync(x => x.ShortCode == shortenedURL) != null)
                shortenedURL = await ResolveCollision(shortenedURL);

            return shortenedURL;
        }
        /// <summary>
        /// solves a collision by adding a suffix to the shortened version of URL.
        /// </summary>
        /// <param name="shortURL"></param>
        /// <returns></returns>
        private async Task<string> ResolveCollision(string shortURL)
        {
            // collision has happened, going to make a new unique URL
            do
            {
                shortURL = _shortenerService.CollisionHandler(ref shortURL);
            }
            while (await _context.URLs.AnyAsync(x => x.ShortCode == shortURL));

            return shortURL;
        }

        private URLDTO URLModelToDTO(URLModel url)
        {
            return _mapper.Map<URLDTO>(url);
        }
    }
}

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Services.Interfaces;
using URLShortenerAPI.Utility.Exceptions;

namespace URLShortenerAPI.Services
{
    internal class URLService : IURLService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IShortenerService _shortenerService;
        private readonly ICacheService _cacheService;
        public URLService(AppDbContext context, IMapper mapper, IAuthService authService, IShortenerService shortenerService, ICacheService cacheService)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _shortenerService = shortenerService;
            _cacheService = cacheService;
        }
        /// <summary>
        /// Adds a new URL to the database after checking if this operation is authorized.
        /// </summary>
        /// <param name="url">The object containing information about the new URL to be added.</param>
        /// <param name="username">The username of the user requesting this operation.</param>
        /// <returns>A <see cref="URLDTO"/> object representing the newly added URL.</returns>
        /// <exception cref="NotAuthorizedException">Thrown when the user is not authorized.</exception>
        /// <exception cref="ArgumentException">Thrown when the URL already exists for the user.</exception>
        /// <exception cref="Exception">Thrown when there's an error saving the URL record.</exception>
        public async Task<URLDTO> AddURL(URLCreateDTO url, string username)
        {
            // Authorize user access. This throws an exception if the user is not authorized.
            UserModel user = await _authService.AuthorizeUserAccessAsync(url.UserID, username);

            // Check if the user has already shortened this URL. Throws an exception if it exists.
            await EnsureURLDoesNotExistAsync(url.LongURL, url.UserID);

            // Create a new URLModel object to be added to the database.
            URLModel newRecord = await CreateNewURLRecord(url, user);

            // Save the new record to the database using a transaction for atomicity.
            await SaveURLRecordWithTransaction(newRecord);

            // We immediately cache it in Redis.
            await _cacheService.SetAsync("URL", newRecord.ShortCode, newRecord);

            return URLModelToDTO(newRecord);
        }

        /// <summary>
        /// Creates a new URLModel DB record.
        /// </summary>
        /// <param name="url">The object containing the new URL's information.</param>
        /// <param name="user">The owner of this URL.</param>
        /// <returns>A new <see cref="URLModel"/> object.</returns>
        private async Task<URLModel> CreateNewURLRecord(URLCreateDTO url, UserModel user)
        {
            // Map the DTO to a new URLModel object.
            URLModel newRecord = _mapper.Map<URLModel>(url);

            // Set the user for this URL.
            newRecord.User = user;

            // Generate a short code for this URL.
            newRecord.ShortCode = await ShortURLGenerator(url.LongURL);

            // If a category is specified, resolve or create it.
            if (!string.IsNullOrEmpty(url.Category))
            {
                var categories = await ResolveOrCreateCategory(url.Category, user);
                newRecord.Categories = categories;

                foreach (var category in categories)
                {
                    // Attach each category to the context
                    _context.Attach(category);

                    // Add the new record to the category's URLs collection
                    category.URLs?.Add(newRecord);
                }
            }
            return newRecord;
        }

        /// <summary>
        /// Saves the URL record to the database using a transaction to ensure atomicity.
        /// </summary>
        /// <param name="newRecord">The URLModel to be saved.</param>
        /// <exception cref="ApplicationException">Thrown when there's an error saving the record.</exception>
        private async Task SaveURLRecordWithTransaction(URLModel newRecord)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add the new URL to the context and save changes.
                await _context.URLs.AddAsync(newRecord);
                await _context.SaveChangesAsync();

                // Commit the transaction if everything succeeded.
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // If an error occurs, roll back the transaction.
                await transaction.RollbackAsync();
                throw new ApplicationException("Failed to save URL record", ex);
            }
        }

        /// <summary>
        /// Resolves the category by name or creates a new one if it does not exist.
        /// </summary>
        /// <param name="categoryString">The name of the category to resolve.</param>
        /// <param name="user">The associated user who owns the category.</param>
        /// <returns>A <see cref="URLCategoryModel"/> representing the resolved or created category.</returns>
        private async Task<List<URLCategoryModel>> ResolveOrCreateCategory(string categoryString, UserModel user)
        {
            var categories = categoryString.Split(',').Select(x => x.Trim());
            List<URLCategoryModel> result = new List<URLCategoryModel>();

            foreach (var x in categories)
            {
                // Attempt to resolve the category by title.
                URLCategoryModel? category = await _context.URLCategories
                                            .FirstOrDefaultAsync(c => c.Title == x);

                // If the category doesn't exist, we create a new one.
                if (category == null)
                {
                    category = new URLCategoryModel
                    {
                        Title = categoryString,
                        User = user,
                        URLs = []
                    };
                    await _context.URLCategories.AddAsync(category);
                }
                result.Add(category);
            }
            return result;
        }

        /// <summary>
        /// Ensures that the given URL does not already exist for the specified user.
        /// </summary>
        /// <param name="longURL">The long URL to check.</param>
        /// <param name="userID">The ID of the owner.</param>
        /// <exception cref="ArgumentException">Thrown when the URL already exists for the user.</exception>
        private async Task EnsureURLDoesNotExistAsync(string longURL, int userID)
        {
            // Check if the URL already exists for this user.
            URLModel? url = await _context.URLs.FirstOrDefaultAsync(x => x.LongURL == longURL);
            if (url == null)
                return;
            else if (url.UserID == userID)
                throw new ArgumentException("URL Already Exists.");
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

        /// <summary>
        /// Removes a URL from Database.
        /// </summary>
        /// <param name="URLID">ID of the URL to remove</param>
        /// <param name="requestingUsername">the username requesting the deletion.</param>
        /// <returns></returns>
        public async Task DeleteURL(int URLID, string requestingUsername)
        {
            var url = await _authService.AuthorizeURLAccessAsync(URLID, requestingUsername);
            _context.URLs.Remove(url);
            return;
        }

        private URLDTO URLModelToDTO(URLModel url)
        {
            return _mapper.Map<URLDTO>(url);
        }
    }
}

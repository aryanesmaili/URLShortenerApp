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

        public URLService(AppDbContext context,
            IMapper mapper,
            IAuthService authService,
            IShortenerService shortenerService,
            ICacheService cacheService)
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
        /// <param name="createDTO">The object containing information about the new URL to be added.</param>
        /// <param name="username">The username of the user requesting this operation.</param>
        /// <returns>A <see cref="URLDTO"/> object representing the newly added URL.</returns>
        /// <exception cref="NotAuthorizedException">Thrown when the user is not authorized.</exception>
        /// <exception cref="ArgumentException">Thrown when the URL already exists for the user.</exception>
        /// <exception cref="Exception">Thrown when there's an error saving the URL record.</exception>
        public async Task<URLShortenResponse> AddURL(URLCreateDTO createDTO, string username)
        {
            // Authorize user access. This throws an exception if the user is not authorized.
            UserModel user = await _authService.AuthorizeUserAccessAsync(createDTO.UserID, username);

            // Check if the user has already shortened this URL. returns answer if yes.
            URLModel? url = await IsURLAlreadyAsync(createDTO.LongURL, createDTO.UserID);

            URLShortenResponse shortenResponse; // the object that will hold the answer.

            if (url != null) // means we already have this URL.
            {
                shortenResponse = new URLShortenResponse() { URL = URLModelToDTO(url), IsNew = false };
                return shortenResponse;
            }

            // Create a new URLModel object to be added to the database.
            URLModel newRecord = await CreateNewURLRecord(createDTO, user);

            // Save the new record to the database using a transaction for atomicity.
            await SaveURLRecordWithTransaction(newRecord);

            // We immediately cache it in Redis.
            await _cacheService.SetAsync(newRecord.ShortCode, newRecord);

            shortenResponse = new()
            { URL = URLModelToDTO(newRecord), IsNew = true };

            return shortenResponse;
        }

        /// <summary>
        /// Adds a batch of URLs to Database.
        /// </summary>
        /// <param name="batchURL">the batch of URLs to be added.</param>
        /// <param name="username">username of the user requesting the Addition.</param>
        /// <returns>a response containing two lists, one for the new URLs that have been added, another for URLs that already existed.</returns>
        public async Task<List<URLShortenResponse>> AddBatchURL(List<URLCreateDTO> batchURL, string username)
        {
            // get userID of the user asking the addition.
            int userID = batchURL[0].UserID;

            // Authorize user access. This throws an exception if the user is not authorized.
            UserModel user = await _authService.AuthorizeUserAccessAsync(userID, username);

            // the HashSet of all the new URLs to be added. later used to pull out the new ones. HashSet helps with avoiding duplicates.
            HashSet<string> AllNewURLs = batchURL.Select(x => x.LongURL).ToHashSet();

            // Check if the user has already shortened any of these URLs. returns the list of already existing URLs.
            HashSet<URLModel> conflictURLs = (await IsURLAlreadyAsync(AllNewURLs, userID)).ToHashSet();

            // getting the HashSet of LongURLs that already exist in our URL. HashSet helps with avoiding duplicates and has faster search time.
            HashSet<string> alreadyExistingURLs = conflictURLs.Select(x => x.LongURL).ToHashSet();

            // Filter unique URLs in batchURL that are not already existing
            List<URLCreateDTO> uniqueItems = batchURL.Where(unique => !alreadyExistingURLs.Contains(unique.LongURL)).ToList();

            // if there are no new URLs, we just return and cache the existing ones.
            if (uniqueItems.Count == 0)
            {
                List<URLShortenResponse> result = conflictURLs.Select(_mapper.Map<URLDTO>).Select(x => new URLShortenResponse() { URL = x, IsNew = false }).ToList();
                await _cacheService.SetRange(conflictURLs.ToList(), "ShortCode");
                return result;
            }

            List<URLModel> newRecords = [];
            // using a loop because concurrency made problems with dbcontext not being thread safe.
            foreach (var item in uniqueItems)
            {
                var record = await CreateNewURLRecord(item, user);
                newRecords.Add(record);
            }

            // we save the new URLs in a transaction to ensure Atomicity(all done or nothing done).
            await SaveURLRecordWithTransaction(newRecords);

            // We cache all of the new URLs in Redis right away.
            await _cacheService.SetRange(newRecords, "ShortCode");

            // Creating a list of new records that were added.
            List<URLDTO> newURLs = newRecords.Select(_mapper.Map<URLDTO>).ToList();

            // Creating a list of already existing URLs that were not added.
            List<URLDTO> conflicts = conflictURLs.Select(_mapper.Map<URLDTO>).ToList();

            // Combine the new and existing URLs into a single response list
            List<URLShortenResponse> response = newURLs
                .Select(url => new URLShortenResponse { URL = url, IsNew = true })
                .Concat(conflicts.Select(url => new URLShortenResponse { URL = url, IsNew = false }))
                .ToList();

            return response;
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
            if (!string.IsNullOrEmpty(url.Categories))
            {
                List<URLCategoryModel> categories = await ResolveOrCreateCategory(url.Categories, user);
                newRecord.Categories = categories;
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
        /// Adds the batch of new Records to database in a transaction.
        /// </summary>
        /// <param name="newRecords">the list containing the new records to be added.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private async Task SaveURLRecordWithTransaction(List<URLModel> newRecords)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Add the new URLs to the context and save changes.
                await _context.URLs.AddRangeAsync(newRecords);
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
            IEnumerable<string> categories = categoryString.Split(',').Select(x => x.Trim());
            List<URLCategoryModel> result = [];

            foreach (var categoryName in categories)
            {
                // Attempt to resolve the category by title.
                URLCategoryModel? category = await _context.URLCategories
                                                .FirstOrDefaultAsync(c => c.Title == categoryName && c.UserID == user.ID);

                // If the category doesn't exist, we create a new one.
                if (category == null)
                {
                    category = new URLCategoryModel
                    {
                        Title = categoryName,
                        User = user,
                        UserID = user.ID,
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
        private async Task<URLModel?> IsURLAlreadyAsync(string longURL, int userID)
        {
            // Check if the URL already exists for this user.
            URLModel? url = await _context.URLs.Include(x => x.Categories).FirstOrDefaultAsync(x => x.LongURL == longURL && x.UserID == userID);
            return url;
        }

        /// <summary>
        /// Checks the database to see if certain URLs exist.
        /// </summary>
        /// <param name="longURLs">the HashSet of URLs to be checked.</param>
        /// <param name="userID">ID of the owner.</param>
        /// <returns></returns>
        private async Task<List<URLModel>> IsURLAlreadyAsync(HashSet<string> longURLs, int userID)
        {
            // Check if the URL already exists for this user.
            List<URLModel> URLs = await _context.URLs.Include(x => x.Categories).AsNoTracking().Where(x => longURLs.Contains(x.LongURL) && x.UserID == userID).ToListAsync();
            return URLs;
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
            // first we update db
            URLModel url = await _authService.AuthorizeURLAccessAsync(URLID, reqUsername);
            url.IsActive = !url.IsActive;
            await _context.SaveChangesAsync();
            // now we update cache:
            if (!url.IsActive) // if the url is no longer active
                await _cacheService.RemoveAsync<URLModel>(url.ShortCode); // we remove the old record.
            else // if it is active now:
                await _cacheService.SetAsync(url.ShortCode, url); // we cache it.
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

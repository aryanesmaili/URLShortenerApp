using AutoMapper;
using Microsoft.EntityFrameworkCore;
using URLShortener.Application.Common.Exceptions;
using URLShortener.Application.Common.Interfaces.Infrastructure;
using URLShortener.Application.Common.Interfaces.Repositories;
using URLShortener.Application.Features.Categories.Interfaces.Repositories;
using URLShortener.Application.Features.Finance.Interfaces.Repositories;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Application.Features.URLs.Interfaces.Repositories;
using URLShortener.Application.Features.URLs.Interfaces.Services;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Application.Features.Users.Interfaces.Repositories;
using URLShortener.Common.Responses;
using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.Entities.URL;
using URLShortener.Domain.Entities.URLCategory;
using URLShortener.Domain.Entities.User;
using URLShortener.Domain.Enums;
using URLShortener.Infrastructure.Services.User;

namespace URLShortener.Infrastructure.Services.URL;

public sealed class URLService : IURLService
{
    private readonly IMapper _mapper;
    private readonly IShortenerService _shortenerService;
    private readonly ICacheService _cacheService;
    private readonly IURLRepository _urlRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFinancialRecordRepository _financialRecordRepository;
    private readonly IUnitOfWork _uow;
    private readonly IURLCategoryRepository _urlCategoryRepository;
    private readonly IServiceTariffService _serviceTariffService;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly UserNotificationService _userNotificationService;

    public URLService(
        IMapper mapper,
        IShortenerService shortenerService,
        ICacheService cacheService,
        IURLRepository urlRepository,
        IUserRepository userRepository,
        IURLCategoryRepository urlCategoryRepository,
        IFinancialRecordRepository financialRecordRepository,
        IUnitOfWork uow,
        IServiceTariffService serviceTariffService,
        IPurchaseRepository purchaseRepository,
        UserNotificationService userNotificationService)
    {
        _urlRepository = urlRepository;
        _mapper = mapper;
        _shortenerService = shortenerService;
        _cacheService = cacheService;
        _urlRepository = urlRepository;
        _userRepository = userRepository;
        _urlCategoryRepository = urlCategoryRepository;
        _financialRecordRepository = financialRecordRepository;
        _uow = uow;
        _serviceTariffService = serviceTariffService;
        _purchaseRepository = purchaseRepository;
        _userNotificationService = userNotificationService;
    }

    #region Add New URL Section

    /// <summary>
    /// Creates a new shortened URL, routing to custom or normal flow based on <see cref="URLCreateDTO.IsCustom"/>.
    /// </summary>
    /// <param name="createDTO">URL creation payload.</param>
    /// <param name="authenticatedUserId">ID of the requesting user.</param>
    /// <returns>The shortened URL response with a flag indicating if it was newly created.</returns>
    public async Task<URLShortenResponse> AddURL(URLCreateDTO createDTO, long authenticatedUserId)
    {
        if (createDTO.IsCustom)
            return await AddCustomURL(createDTO, authenticatedUserId);
        else
            return await AddNormalShortURL(createDTO, authenticatedUserId);
    }

    /// <summary>
    /// Creates a custom short-coded URL after validating short code availability,
    /// duplicate long URLs, and user balance.
    /// </summary>
    /// <param name="createDTO">URL creation payload including the custom short code.</param>
    /// <param name="authenticatedUserId">ID of the requesting user.</param>
    /// <returns>The shortened URL response with a flag indicating if it was newly created.</returns>
    /// <exception cref="ArgumentException">Thrown when the custom short code is already taken.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    /// <exception cref="InsufficientBalanceException">Thrown when the user's balance can't cover the service cost.</exception>
    private async Task<URLShortenResponse> AddCustomURL(URLCreateDTO createDTO, long authenticatedUserId)
    {
        bool isAlreadyInUse = await CheckCustomShortCodeInUse(createDTO.CustomShortCode!);
        if (isAlreadyInUse)
            throw new ArgumentException("Entered Custom Short Code is Already in Use by another URL");

        // Return early if the user already has a short URL for this long URL.
        URLModel? alreadyExistingURL = await GetAlreadyShortenedItem(createDTO.LongURL, authenticatedUserId);
        if (alreadyExistingURL != null)
            return new URLShortenResponse { URL = _mapper.Map<URLDTO>(alreadyExistingURL), IsNew = false };

        var servicePrice = await GetServiceCost(ServiceType.CustomShortCode);
        UserModel user = await GetUserModel(authenticatedUserId)
            ?? throw new NotFoundException(nameof(UserModel), nameof(UserModel.ID), authenticatedUserId);

        if (user.FinancialRecord.Balance < servicePrice)
            throw new InsufficientBalanceException($"User's balance is insufficient. Required: {servicePrice}");

        URLModel urlToAdd = await CreateCustomURLModel(createDTO, authenticatedUserId);
        PurchaseModel purchase = MakePurchaseModel(user, urlToAdd, ServiceType.CustomShortCode, servicePrice);

        await SubmitNewCustomURLByTransaction(urlToAdd, user, purchase);
        await SendSignalRNotifications(user.ID, servicePrice);
        await SetNewURLInCache(urlToAdd);

        return new URLShortenResponse() { URL = _mapper.Map<URLDTO>(urlToAdd), IsNew = true };
    }

    /// <summary>
    /// Sends real-time SignalR notifications for balance and URL count changes.
    /// </summary>
    /// <param name="userId">Target user ID.</param>
    /// <param name="balanceUpdateAmount">New balance value to broadcast. Skipped if null.</param>
    /// <param name="urlCountDelta">Number of URLs added or removed. Defaults to 1.</param>
    private async Task SendSignalRNotifications(long userId, long? balanceUpdateAmount = null, int urlCountDelta = 1)
    {
        if (balanceUpdateAmount != null)
            await _userNotificationService.NotifyBalanceChanged(userId, balanceUpdateAmount.Value);

        await _userNotificationService.NotifyUrlCountChanged(userId, urlCountDelta);
    }

    /// <summary>Gets the price for the given service type.</summary>
    private async Task<long> GetServiceCost(ServiceType serviceType)
        => await _serviceTariffService.GetServicePrice(serviceType);

    /// <summary>Fetches the user model with their financial record eagerly loaded.</summary>
    private async Task<UserModel?> GetUserModel(long userId)
        => await _userRepository.GetAsync(x => x.ID == userId, i => i.Include(u => u.FinancialRecord));

    /// <summary>
    /// Builds a <see cref="PurchaseModel"/> linking the URL, user's financial record, and service cost.
    /// </summary>
    /// <param name="user">The purchasing user.</param>
    /// <param name="url">The URL being purchased.</param>
    /// <param name="serviceType">The type of service being charged.</param>
    /// <param name="servicePrice">The charge amount.</param>
    /// <returns>An unpersisted <see cref="PurchaseModel"/>.</returns>
    private static PurchaseModel MakePurchaseModel(UserModel user, URLModel url, ServiceType serviceType, long servicePrice)
        => new()
        {
            URL = url,
            ServiceType = (int)serviceType,
            Amount = servicePrice,
            FinanceID = user.FinancialRecord.ID,
            Finance = user.FinancialRecord,
            CreatedAt = DateTime.UtcNow,
        };

    /// <summary>
    /// Maps <paramref name="createDTO"/> to a <see cref="URLModel"/>, assigns the custom short code,
    /// and resolves or creates categories if provided.
    /// </summary>
    /// <param name="createDTO">URL creation payload.</param>
    /// <param name="userId">Owner's user ID.</param>
    /// <returns>An unpersisted <see cref="URLModel"/>.</returns>
    private async Task<URLModel> CreateCustomURLModel(URLCreateDTO createDTO, long userId)
    {
        URLModel newRecord = _mapper.Map<URLModel>(createDTO);
        newRecord.CreatedAt = DateTime.UtcNow;
        newRecord.ShortCode = createDTO.CustomShortCode!;

        if (createDTO.HasCategories)
        {
            var categories = await ResolveOrCreateCategories(createDTO.Categories!, userId);
            newRecord.Categories = categories;
        }

        return newRecord;
    }

    /// <summary>
    /// Persists a custom URL, deducts the purchase, and saves all changes atomically.
    /// </summary>
    /// <param name="newURL">The custom URL to persist.</param>
    /// <param name="ownerUser">The user being charged.</param>
    /// <param name="purchaseInfo">The associated purchase record.</param>
    /// <exception cref="InvalidOperationException">Thrown when the transaction fails.</exception>
    private async Task SubmitNewCustomURLByTransaction(URLModel newURL, UserModel ownerUser, PurchaseModel purchaseInfo)
    {
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            CommitPurchase(ownerUser, purchaseInfo);
            _urlRepository.Add(newURL);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await _uow.RollbackTransactionAsync();
            // TODO: inject and use a logger here.
            throw new InvalidOperationException(e.Message, e);
        }
    }

    /// <summary>Writes the URL's short code and model to the cache.</summary>
    private async Task SetNewURLInCache(URLModel newURL)
        => await _cacheService.SetAsync(newURL.ShortCode, newURL);

    /// <summary>
    /// Creates a normal (non-custom) shortened URL, skipping duplicates and persisting atomically.
    /// </summary>
    /// <param name="createDTO">URL creation payload.</param>
    /// <param name="userId">ID of the requesting user.</param>
    /// <returns>The shortened URL response with a flag indicating if it was newly created.</returns>
    private async Task<URLShortenResponse> AddNormalShortURL(URLCreateDTO createDTO, long userId)
    {
        // Return early if the user already has a short URL for this long URL.
        URLModel? alreadyExistingURL = await GetAlreadyShortenedItem(createDTO.LongURL, userId);
        if (alreadyExistingURL != null)
            return new URLShortenResponse { URL = _mapper.Map<URLDTO>(alreadyExistingURL), IsNew = false };

        URLModel urlToAdd = await CreateNormalURLModel(createDTO, userId);

        await SubmitNewNormalURLByTransaction(urlToAdd);
        await SendSignalRNotifications(userId);
        await SetNewURLInCache(urlToAdd);

        return new URLShortenResponse() { URL = _mapper.Map<URLDTO>(urlToAdd), IsNew = true };
    }

    /// <summary>
    /// Maps <paramref name="createDTO"/> to a <see cref="URLModel"/>, generates a short code,
    /// and resolves or creates categories if provided.
    /// </summary>
    /// <param name="createDTO">URL creation payload.</param>
    /// <param name="authenticatedUserId">Owner's user ID.</param>
    /// <returns>An unpersisted <see cref="URLModel"/>.</returns>
    private async Task<URLModel> CreateNormalURLModel(URLCreateDTO createDTO, long authenticatedUserId)
    {
        URLModel newRecord = _mapper.Map<URLModel>(createDTO);
        newRecord.CreatedAt = DateTime.UtcNow;
        newRecord.ShortCode = await ShortURLGenerator(createDTO.LongURL);

        if (createDTO.HasCategories)
        {
            List<URLCategoryModel> categories = await ResolveOrCreateCategories(createDTO.Categories!, authenticatedUserId);
            newRecord.Categories = categories;
        }

        return newRecord;
    }

    /// <summary>
    /// Persists a normal URL within a transaction, rolling back on failure.
    /// </summary>
    /// <param name="newRecord">The URL model to persist.</param>
    /// <exception cref="InvalidOperationException">Thrown when the transaction fails.</exception>
    private async Task SubmitNewNormalURLByTransaction(URLModel newRecord)
    {
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            _urlRepository.Add(newRecord);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await _uow.RollbackTransactionAsync();
            // TODO: inject and use a logger here.
            throw new InvalidOperationException(e.Message, e);
        }
    }

    /// <summary>
    /// Records the purchase, deducts the amount from the user's balance, and flags the financial record for update.
    /// </summary>
    /// <param name="user">The user being charged.</param>
    /// <param name="purchaseInfo">The purchase to record.</param>
    private void CommitPurchase(UserModel user, PurchaseModel purchaseInfo)
    {
        _purchaseRepository.Add(purchaseInfo);
        user.FinancialRecord.Purchases.Add(purchaseInfo);
        user.FinancialRecord.Balance -= purchaseInfo.Amount;
        _financialRecordRepository.Update(user.FinancialRecord);
    }

    /// <summary>
    /// Shortens a batch of URLs, skipping duplicates, persisting new entries atomically,
    /// and caching results.
    /// </summary>
    /// <param name="batchURL">Batch creation payload containing one or more URLs.</param>
    /// <param name="authenticatedUserId">ID of the requesting user.</param>
    /// <returns>
    /// A combined list of <see cref="URLShortenResponse"/> for both newly created
    /// and pre-existing URLs.
    /// </returns>
    public async Task<IReadOnlyList<URLShortenResponse>> AddBatchURL(BatchURLCreateDTO batchURL, long authenticatedUserId)
    {
        // Deduplicate input by long URL before any DB work.
        var distinctBatch = batchURL.URLs
            .DistinctBy(x => x.LongURL)
            .ToList();

        var allNewURLs = distinctBatch
            .Select(x => x.LongURL)
            .ToHashSet();

        // Find which of the submitted long URLs are already shortened by this user.
        var conflictURLs = await GetAlreadyShortenedItems(allNewURLs, authenticatedUserId);

        var alreadyExistingURLs = conflictURLs
            .Select(x => x.LongURL)
            .ToHashSet();

        // Only process URLs not already in the system.
        var uniqueItems = distinctBatch
            .Where(x => !alreadyExistingURLs.Contains(x.LongURL))
            .ToList();

        // All submitted URLs already exist — cache and return without inserting.
        if (uniqueItems.Count == 0)
        {
            List<URLShortenResponse> result = conflictURLs
                .Select(url => new URLShortenResponse() { URL = _mapper.Map<URLDTO>(url), IsNew = false })
                .ToList();

            await _cacheService.SetRange(conflictURLs, x => x.ShortCode);
            return result;
        }

        BatchURLCreateDTO createDTO = new(batchURL, uniqueItems);
        IReadOnlyList<URLModel> newRecords = await CreateBatchURLModels(createDTO, authenticatedUserId);

        await SaveBatchURLRecordWithTransaction(newRecords);
        await _cacheService.SetRange(newRecords, x => x.ShortCode);
        await SendSignalRNotifications(authenticatedUserId, urlCountDelta: newRecords.Count);

        var newURLs = newRecords
            .Select(x => new URLShortenResponse { URL = _mapper.Map<URLDTO>(x), IsNew = true })
            .ToList();

        var conflicts = conflictURLs
            .Select(x => new URLShortenResponse { URL = _mapper.Map<URLDTO>(x), IsNew = false })
            .ToList();

        // Merge new and pre-existing URLs into a single response.
        return [.. newURLs, .. conflicts];
    }

    /// <summary>
    /// Maps batch DTOs to <see cref="URLModel"/> instances, generates unique short codes,
    /// and assigns shared categories if specified.
    /// </summary>
    /// <param name="batch">The batch payload to process.</param>
    /// <param name="authenticatedUserId">Owner's user ID.</param>
    /// <returns>A list of unpersisted <see cref="URLModel"/> instances with short codes assigned.</returns>
    private async Task<IReadOnlyList<URLModel>> CreateBatchURLModels(BatchURLCreateDTO batch, long authenticatedUserId)
    {
        var createTime = DateTime.UtcNow;

        var modelItems = batch.URLs.Select(x =>
        {
            URLModel newItem = _mapper.Map<URLModel>(x);
            newItem.CreatedAt = createTime;
            newItem.IsActive = batch.IsActive;
            newItem.IsMonetized = batch.IsMonetized;
            return newItem;
        }).ToList();

        modelItems = await BatchShortURLGenerator(modelItems);

        // All items in a batch share the same categories.
        if (batch.Categories.Count != 0)
        {
            List<URLCategoryModel> categories = await ResolveOrCreateCategories(batch.Categories, authenticatedUserId);
            foreach (var item in modelItems)
                item.Categories = categories;
        }

        return modelItems;
    }

    /// <summary>
    /// Persists a batch of URL models within a single transaction, rolling back on failure.
    /// </summary>
    /// <param name="newRecords">The URL models to persist.</param>
    /// <exception cref="InvalidOperationException">Thrown when the transaction fails.</exception>
    private async Task SaveBatchURLRecordWithTransaction(IReadOnlyList<URLModel> newRecords)
    {
        using var transaction = await _uow.BeginTransactionAsync();
        try
        {
            _urlRepository.AddRange(newRecords);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();
        }
        catch (Exception e)
        {
            await _uow.RollbackTransactionAsync();
            // TODO: inject and use a logger here.
            throw new InvalidOperationException(e.Message, e);
        }
    }

    /// <summary>
    /// Returns existing category models for recognized titles and creates new ones for unknown titles.
    /// Normalizes input by trimming, deduplicating case-insensitively, and removing empty entries.
    /// </summary>
    /// <param name="categoryString">Raw category title strings from the request.</param>
    /// <param name="userId">Owner's user ID, used to scope category lookups.</param>
    /// <returns>A merged list of existing and newly created <see cref="URLCategoryModel"/> instances.</returns>
    private async Task<List<URLCategoryModel>> ResolveOrCreateCategories(IReadOnlyCollection<string> categoryString, long userId)
    {
        var categories = categoryString
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Single query to fetch all matching categories for this user.
        var dbCategories = await _urlCategoryRepository
            .Query()
            .Where(x => categories.Contains(x.Title) && x.UserID == userId)
            .ToListAsync();

        // HashSet for O(1) lookups when diffing against the input list.
        var existingTitles = dbCategories
            .Select(x => x.Title)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newCategories = categories
            .Where(x => !existingTitles.Contains(x))
            .Select(x => new URLCategoryModel { Title = x, UserID = userId, URLs = [] })
            .ToList();

        return [.. dbCategories, .. newCategories];
    }

    /// <summary>Fetches a user's existing shortened entry for the given long URL, with categories loaded.</summary>
    private async Task<URLModel?> GetAlreadyShortenedItem(string longURL, long userId)
        => await _urlRepository.GetAsync(x => x.LongURL == longURL && x.UserID == userId, i => i.Include(r => r.Categories));

    /// <summary>
    /// Fetches all of a user's existing shortened entries that match any URL in <paramref name="longURLs"/>.
    /// Queries in chunks of 1000 to avoid SQL parameter limits.
    /// </summary>
    /// <param name="longURLs">The long URLs to check for conflicts.</param>
    /// <param name="userId">The user to scope the lookup to.</param>
    /// <returns>All matching <see cref="URLModel"/> records with categories loaded.</returns>
    private async Task<IReadOnlyList<URLModel>> GetAlreadyShortenedItems(IEnumerable<string> longURLs, long userId)
    {
        // Materialize once to avoid multiple enumeration and ensure EF translates to SQL IN.
        var urlList = longURLs as List<string> ?? longURLs.ToList();
        var conflictURLs = new List<URLModel>();

        foreach (var chunk in urlList.Chunk(1000))
        {
            var batchConflicts = await _urlRepository
                .Query()
                .Include(x => x.Categories)
                .AsNoTracking()
                .Where(x => chunk.Contains(x.LongURL) && x.UserID == userId)
                .ToListAsync();

            conflictURLs.AddRange(batchConflicts);
        }

        return conflictURLs;
    }

    /// <summary>Returns true if the given short code is already assigned to any URL.</summary>
    private async Task<bool> CheckCustomShortCodeInUse(string shortCode)
        => await _urlRepository.AnyAsync(x => x.ShortCode == shortCode);

    /// <summary>
    /// Generates a unique short code for the given URL, resolving collisions if necessary.
    /// </summary>
    /// <param name="longURL">The original URL to shorten.</param>
    /// <param name="maxRetries">Maximum collision resolution attempts before throwing. Defaults to 10.</param>
    /// <returns>A unique short code that doesn't exist in the repository.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a unique code cannot be generated within <paramref name="maxRetries"/> attempts.</exception>
    private async Task<string> ShortURLGenerator(string longURL, int maxRetries = 10)
    {
        string shortenedURL = _shortenerService.HashURL(longURL);

        if (!await _urlRepository.AnyAsync(x => x.ShortCode == shortenedURL))
            return shortenedURL;

        return await ResolveCollision(shortenedURL, maxRetries);
    }

    /// <summary>
    /// Resolves a short code collision by appending random suffixes of increasing length.
    /// Suffix length increases every 3 attempts to expand the candidate space.
    /// </summary>
    /// <param name="shortURL">The colliding short code to resolve.</param>
    /// <param name="maxRetries">Maximum number of resolution attempts.</param>
    /// <returns>A unique short code with an appended suffix.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the collision cannot be resolved within <paramref name="maxRetries"/> attempts.</exception>
    private async Task<string> ResolveCollision(string shortURL, int maxRetries)
    {
        int suffixLength = 1;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            string candidate = _shortenerService.CollisionHandler(shortURL, suffixLength);

            if (!await _urlRepository.AnyAsync(x => x.ShortCode == candidate))
                return candidate;

            // Widen suffix every 3 attempts to escape collision clusters.
            if ((attempt + 1) % 3 == 0)
                suffixLength++;
        }

        throw new InvalidOperationException(
            $"Failed to generate unique short code after {maxRetries} attempts. Hash space may be saturated.");
    }

    /// <summary>
    /// Generates unique short codes for a batch of URL models, handling hash collisions
    /// both within the batch itself and against existing records in the database.
    /// </summary>
    /// <param name="items">
    /// The list of <see cref="URLModel"/> objects whose <c>ShortCode</c> property will be populated.
    /// </param>
    /// <param name="maxRetries">
    /// Maximum number of collision-resolution attempts before giving up.
    /// Suffix length increases every 3 attempts to widen the candidate space.
    /// </param>
    /// <returns>
    /// The same items with their <c>ShortCode</c> properties set to globally unique values.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more items could not be assigned a unique short code
    /// within the allowed number of retries.
    /// </exception>
    private async Task<List<URLModel>> BatchShortURLGenerator(
        IReadOnlyList<URLModel> items,
        int maxRetries = 10)
    {
        // -------------------------------------------------------------------------
        // STEP 1 — Compute the initial hash for every item.
        // -------------------------------------------------------------------------
        // We assign each item its first-pass short code based on its long URL.
        // At this point we don't know yet whether these codes are taken or not.
        foreach (var item in items)
            item.ShortCode = _shortenerService.HashURL(item.LongURL);

        // -------------------------------------------------------------------------
        // STEP 2 — Ask the DB which of the initial codes already exist.
        // -------------------------------------------------------------------------
        // We collect distinct codes so we don't send duplicate values to the DB,
        // then fetch only the ones that are already taken.
        // We collect distinct codes and query the DB in chunks to avoid
        // exceeding SQL parameter limits.
        var initialCodes = items.Select(x => x.ShortCode).Distinct().ToList();
        var existingInDb = new List<string>();
        foreach (var chunk in initialCodes.Chunk(1000))
        {
            var batchHits = await _urlRepository.Query()
                .Where(x => chunk.Contains(x.ShortCode))
                .Select(x => x.ShortCode)
                .ToListAsync();
            existingInDb.AddRange(batchHits);
        }


        // -------------------------------------------------------------------------
        // STEP 3 — Build the global "taken" set.
        // -------------------------------------------------------------------------
        // This HashSet tracks every code that is unavailable — either because it
        // exists in the DB or because another item in this same batch already
        // claimed it. Using HashSet.Add() as the conflict gate is O(1) per check.
        var takenCodes = new HashSet<string>(existingInDb);

        // -------------------------------------------------------------------------
        // STEP 4 — First-pass resolution: resolve items that have no conflict.
        // -------------------------------------------------------------------------
        // We walk through each item in order. If its current ShortCode can be
        // inserted into takenCodes (i.e. it wasn't already there), the item is
        // considered resolved and we move on. Otherwise it goes into the retry
        // queue. This single loop handles BOTH db conflicts and intra-batch
        // duplicates in one pass.
        var toResolve = new List<URLModel>(); // items still needing a unique code

        foreach (var item in items)
        {
            // HashSet.Add returns false when the value is already present,
            // which means some other item (in DB or earlier in this batch)
            // already owns this code.
            if (!takenCodes.Add(item.ShortCode))
                toResolve.Add(item);
            // If Add returned true, the code is now claimed by this item — nothing
            // else to do; item.ShortCode already holds the correct final value.
        }

        // -------------------------------------------------------------------------
        // STEP 5 — Collision resolution loop.
        // -------------------------------------------------------------------------
        // For every remaining conflicted item we ask CollisionHandler to produce
        // an alternative candidate derived from the current code. We batch-query
        // the DB for all new candidates at once, then check intra-batch uniqueness
        // the same way as step 4. We repeat until every item is resolved or we
        // exhaust our retry budget.
        //
        // suffixLength grows every 3 attempts so that later attempts explore a
        // larger search space (e.g. longer random suffixes), reducing the chance
        // of repeated collisions under high load.
        int suffixLength = 1;

        for (int attempt = 0; attempt < maxRetries && toResolve.Count > 0; attempt++)
        {
            // Widen the suffix every 3 attempts to escape collision clusters.
            if ((attempt + 1) % 3 == 0) suffixLength++;

            // -- 5a. Generate one candidate per still-unresolved item ----------
            // We mutate ShortCode in place so the next CollisionHandler call
            // always derives from the most recent candidate, not the original hash.
            // This prevents two items from repeatedly generating the same candidate.
            foreach (var item in toResolve)
                item.ShortCode = _shortenerService.CollisionHandler(item.ShortCode, suffixLength);

            // -- 5b. Batch-query the DB for all new candidates -----------------
            // Distinct() avoids redundant DB lookups when two items happened to
            // collide with each other and produced identical new candidates.
            // We query in chunks to prevent SQL parameter limit exceptions.
            var toQuery = toResolve.Select(x => x.ShortCode).Distinct().ToList();
            var dbHits = new List<string>();
            foreach (var chunk in toQuery.Chunk(1000))
            {
                var batchHits = await _urlRepository.Query()
                    .Where(x => chunk.Contains(x.ShortCode))
                    .Select(x => x.ShortCode)
                    .ToListAsync();
                dbHits.AddRange(batchHits);
            }

            // Add every DB hit to the global taken set so step 5c can see them.
            foreach (var code in dbHits)
                takenCodes.Add(code);

            // -- 5c. Check each candidate against the taken set ----------------
            // Same pattern as step 4: HashSet.Add is our atomic "claim or reject".
            var stillToResolve = new List<URLModel>();
            foreach (var item in toResolve)
            {
                if (!takenCodes.Add(item.ShortCode))
                    // Code is still taken — keep item in the queue for next round.
                    stillToResolve.Add(item);
                // Otherwise the code is now claimed; item.ShortCode is already set.
            }

            toResolve = stillToResolve;
        }

        // -------------------------------------------------------------------------
        // STEP 6 — Guard: fail loudly if any item couldn't be resolved.
        // -------------------------------------------------------------------------
        // This should only happen under extreme collision pressure. The caller
        // should decide whether to retry the whole operation or surface an error.
        if (toResolve.Count > 0)
            throw new InvalidOperationException(
                $"Failed to resolve unique short codes for {toResolve.Count} URL(s) " +
                $"after {maxRetries} attempts. Consider increasing maxRetries or " +
                $"reviewing the collision handler strategy.");

        // All items now have their ShortCode set to a globally unique value.
        return items.ToList();
    }

    #endregion

    /// <summary>
    /// Get a URL's Info from database.
    /// </summary>
    /// <param name="urlID">ID of the URL to fetch.</param>
    /// <returns>a <see cref="URLDTO"/>object containing info about the record.</returns>
    /// <exception cref="NotFoundException">thrown if no object with this ID exists.</exception>
    public async Task<URLDTO> GetURLAsync(long urlID)
    {
        URLModel url = await _urlRepository
            .GetAsync(x => x.ID == urlID, asNoTracking: true)
            ?? throw new NotFoundException(nameof(URLModel), nameof(URLModel.ID), urlID);
        return _mapper.Map<URLDTO>(url);
    }

    public async Task<PagedResult<URLDTO>> GetPagedURLsAsync(long userId, int pageNumber, int pageSize)
    {
        var URLs = await _urlRepository.GetPagedAsync(pageNumber, pageSize, x => x.UserID == userId, x => x.CreatedAt, descending: true);
        return _mapper.Map<PagedResult<URLDTO>>(URLs); // TODO: consider using CreateResult across project
    }

    /// <summary>
    /// Mutates a specific state of the URL, updates the database, and refreshes the cache.
    /// </summary>
    public async Task ToggleStateAsync(long urlId, Action<URLModel> toggleAction, long userId)
    {
        // 1. Fetch from DB
        URLModel url = await _urlRepository
            .GetAsync(x => x.ID == urlId && x.UserID == userId)
            ?? throw new NotFoundException($"URL {urlId} Does not Exist");

        // 2. Flip state
        toggleAction(url);

        // 3. Update DB
        _urlRepository.Update(url);
        await _uow.SaveChangesAsync();

        // 4. Update Cache
        await _cacheService.SetAsync(url.ShortCode, url, strategy: CacheStrategy.Always);
    }

    public async Task DeleteURL(long urlId, long userId)
    {
        URLModel url = await _urlRepository
            .GetAsync(x => x.ID == urlId && x.UserID == userId)
            ?? throw new NotFoundException(nameof(URLModel), nameof(URLModel.ID), urlId);
        _urlRepository.Remove(url);
        await _uow.SaveChangesAsync();
        await SendSignalRNotifications(url.UserID);
        await _cacheService.RemoveAsync<UserStats>("UserStats_" + url.UserID); // so that if the user refreshes, their stats be calculated again.
    }

}

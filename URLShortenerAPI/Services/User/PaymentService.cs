using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.Finance;
using URLShortenerAPI.Data.Entities.User;
using URLShortenerAPI.Data.Interfaces.User;
using URLShortenerAPI.Utility.Exceptions;
using URLShortenerAPI.Utility.SignalR;

namespace URLShortenerAPI.Services.User
{
    internal class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IZibalService _zibalService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IHubContext<UserBalanceHub> _balancehubContext;
        private readonly UserConnectionMapping _connectionMapping;

        private const string _callbackURL = "https://www.Pexita.Click/Payments/callback";
        private const string _merchantName = "Pexita";
        public PaymentService(AppDbContext context,
                              IZibalService zibalService,
                              IAuthService authService,
                              IMapper mapper,
                              IHubContext<UserBalanceHub> balancehubContext,
                              UserConnectionMapping connectionMapping)
        {
            _context = context;
            _zibalService = zibalService;
            _authService = authService;
            _mapper = mapper;
            _balancehubContext = balancehubContext;
            _connectionMapping = connectionMapping;
        }

        /// <summary>
        /// Gets the list of a user's deposits.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="username"></param>
        /// <returns>List of <see cref="DepositDTO"/></returns>
        public async Task<List<DepositDTO>> GetDepositsAsync(int userID, string username)
        {
            UserModel user = await _authService.AuthorizeUserDepositsAccess(userID, username);

            return user.FinancialRecord.Deposits.Select(_mapper.Map<DepositDTO>).ToList();
        }

        /// <summary>
        /// Initiates a new transaction procedure.
        /// </summary>
        /// <param name="paymentCreate"></param>
        /// <param name="username"></param>
        /// <returns>The response sent by zibal.</returns>
        public async Task<CreateTransactionResponse> CreateTransactionAsync(PaymentCreateDTO paymentCreate, string username)
        {
            UserModel user = await _authService.AuthorizeUserAccessAsync(paymentCreate.UserID, username, includeRelations: true);
            string orderID = GenerateRandomOrderID(user.ID);

            CreateTransactionRequest createTransaction = new()
            {
                Amount = paymentCreate.Amount,
                CallbackURL = _callbackURL,
                Merchant = _merchantName,
                Description = paymentCreate.Description,
                Mobile = paymentCreate.Mobile,
                OrderID = orderID
            };

            CreateTransactionResponse response = await _zibalService.RequestTransactionAsync(createTransaction);
            DepositModel deposit;
            if (response.Result == 100)
            {
                deposit = new()
                {
                    FinanceID = user.FinancialID,
                    Finance = user.FinancialRecord,
                    Amount = createTransaction.Amount,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccessful = true,
                    TrackID = response.TrackID,
                    OrderID = orderID
                };
            }
            else
            {
                deposit = new()
                {
                    Amount = createTransaction.Amount,
                    Finance = user.FinancialRecord,
                    CreatedAt = DateTime.UtcNow,
                    FinanceID = user.FinancialID,
                    IsSuccessful = false,
                    FailureReason = response.Message,
                    OrderID = orderID
                };
            }
            await _context.Deposits.AddAsync(deposit);
            await _context.SaveChangesAsync();
            return response;
        }

        /// <summary>
        /// Verifies a transaction by sending it's trackID to Zibal.
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        /// <exception cref="NotFoundException"></exception>
        public async Task<VerifyTransactionResponse> VerifyTransactionAsync(long trackID)
        {
            VerifyTransactionRequest request = new()
            {
                Merchant = _merchantName,
                TrackID = trackID
            };
            VerifyTransactionResponse response = await _zibalService.VerifyTransactionAsync(request);

            if (response.Result == 100)
            {
                DepositModel deposit = await _context.Deposits.FirstOrDefaultAsync(x => x.OrderID == response.OrderID) ?? throw new NotFoundException($"No Deposit with orderID {response.OrderID} was found.");
                deposit.RefNumber = response.RefNumber;
                deposit.Description = response.Description;
                deposit.PaidAt = response.PaidAt;
                UserModel user = await _context.Users.Include(x => x.FinancialRecord).FirstOrDefaultAsync(x => x.FinancialID == deposit.FinanceID) ?? throw new NotFoundException($"User with financial id {deposit.FinanceID} Does Not Exist.");
                user.FinancialRecord.Balance += response.Amount;
                _context.Update(deposit);
                _context.Update(user);
                await _context.SaveChangesAsync();

                foreach (var connectionID in _connectionMapping.GetConnections(user.Username)) // notify clients that the balance has changed.
                {
                    await _balancehubContext.Clients.Client(connectionID).SendAsync("ReceiveBalanceUpdate", response.Amount);
                }
            }
            return response;
        }

        /// <summary>
        /// Gets the status of a transaction from zibal.
        /// </summary>
        /// <param name="trackID"></param>
        /// <returns></returns>
        public async Task<InquiryTransactionResponse> CheckTransactionStatusAsync(int trackID)
        {
            InquiryTransactionRequest request = new() { Merchant = _merchantName, TrackID = trackID };
            InquiryTransactionResponse response = await _zibalService.GetTransactionStatusAsync(request);
            return response;
        }

        /// <summary>
        /// Generates a OrderID for the deposit to happen.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        private static string GenerateRandomOrderID(int userID)
        {
            return $"{userID}_{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }
}

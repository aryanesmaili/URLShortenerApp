using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using URLShortenerAPI.Data.Interfaces.User;

namespace URLShortenerAPI.Services.User
{
    internal class ZibalService : IZibalService
    {
        private const string zibalRequestTransactionAddress = "https://gateway.zibal.ir/v1/request";
        private const string zibalVerifyTransactionAddress = "https://gateway.zibal.ir/v1/verify";
        private const string zibalInquiryTransactionAddress = "https://gateway.zibal.ir/v1/inquiry";
        private const string zibalLazyRequestTransactionAddress = "https://gateway.zibal.ir/request/lazy";

        private readonly HttpClient _httpClient;

        public ZibalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Starts a transaction by sending the required info provided in <see cref="CreateTransactionRequest"/> or it's descendant <see cref="CreateAdvancedTransactionRequest"/>.
        /// </summary>
        /// <param name="transactionInfo">Information about the transaction.</param>
        /// <param name="isLazy">true for transaction in lazy mode, false for normal method.</param>
        /// <param name="isAdvanced">true for transaction in advanced mode.</param>
        /// <returns>a <see cref="CreateTransactionResponse"/> if in normal mode, and a <see cref="CreateAdvancedTransactionResponse"/> if in Advanced mode.</returns>
        public async Task<CreateTransactionResponse> RequestTransactionAsync(CreateTransactionRequest transactionInfo, bool isLazy = false, bool isAdvanced = false)
        {
            string url = isLazy ? zibalLazyRequestTransactionAddress : zibalRequestTransactionAddress;
            HttpRequestMessage request = new(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(transactionInfo), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(request);
            CreateTransactionResponse? zibalResponse;

            if (!isAdvanced)
                zibalResponse = await JsonSerializer.DeserializeAsync<CreateTransactionResponse>(await response.Content.ReadAsStreamAsync());
            else
                zibalResponse = await JsonSerializer.DeserializeAsync<CreateAdvancedTransactionResponse>(await response.Content.ReadAsStreamAsync());

            return zibalResponse!;
        }

        /// <summary>
        /// Verifies a transaction by sending a Post Request to Zibal based on the provided
        /// </summary>
        /// <param name="verifyTransactionRequest">the information needed to verify the request.</param>
        /// <param name="isAdvanced">true for transaction in advanced mode.</param>
        /// <returns>a <see cref="VerifyTransactionResponse"/> object if in normal mode, a <see cref="VerifyAdvancedTransactionResponse"/> if in advanced mode.</returns>
        public async Task<VerifyTransactionResponse> VerifyTransactionAsync(VerifyTransactionRequest verifyTransactionRequest, bool isAdvanced = false)
        {

            HttpRequestMessage request = new(HttpMethod.Post, zibalVerifyTransactionAddress)
            {
                Content = new StringContent(JsonSerializer.Serialize(verifyTransactionRequest), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            VerifyTransactionResponse? zibalResponse;

            if (!isAdvanced)
                zibalResponse = await JsonSerializer.DeserializeAsync<VerifyTransactionResponse>(await response.Content.ReadAsStreamAsync());
            else
                zibalResponse = await JsonSerializer.DeserializeAsync<VerifyAdvancedTransactionResponse>(await response.Content.ReadAsStreamAsync());

            return zibalResponse!;
        }

        /// <summary>
        /// Checks the status of a transaction by sending a post request to Zibal.
        /// </summary>
        /// <param name="inquiryTransactionRequest"></param>
        /// <param name="isAdvanced">true for transaction in advanced mode.</param>
        /// <returns>a <see cref="InquiryTransactionRequest"/> if in normal mode, a <see cref="InquiryAdvancedTransactionResponse"/> if in advanced mode.</returns>
        public async Task<InquiryTransactionResponse> GetTransactionStatusAsync(InquiryTransactionRequest inquiryTransactionRequest, bool isAdvanced = false)
        {
            HttpRequestMessage request = new(HttpMethod.Post, zibalInquiryTransactionAddress)
            {
                Content = new StringContent(JsonSerializer.Serialize(inquiryTransactionRequest), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            InquiryTransactionResponse? zibalResponse;

            if (!isAdvanced)
                zibalResponse = await JsonSerializer.DeserializeAsync<InquiryTransactionResponse>(await response.Content.ReadAsStreamAsync());
            else
                zibalResponse = await JsonSerializer.DeserializeAsync<InquiryAdvancedTransactionResponse>(await response.Content.ReadAsStreamAsync());

            return zibalResponse!;
        }
    }

    /// <summary>
    /// The placeholder for the request that will be sent to Zibal.
    /// </summary>
    internal class CreateTransactionRequest
    {
        private string _merchant = string.Empty;
        /// <summary>
        /// provides access to the merchant. if this is a test, returns "zibal" else the merchant.
        /// </summary>
        public required string Merchant { get => IsTest ? "zibal" : _merchant; set => _merchant = value.Trim(); }

        /// <summary>
        /// is this transaction a test one?
        /// </summary>
        [JsonIgnore]
        public bool IsTest { get; set; } = false;

        /// <summary>
        /// Amount to pay (in Rials)
        /// </summary>
        public required long Amount { get; set; }


        private string _callbackURL = string.Empty;
        /// <summary>
        /// The Address that will receive Zibal's response.
        /// </summary>
        public required string CallbackURL { get => _callbackURL; set => _callbackURL = value.Trim(); }


        private string? _description;
        /// <summary>
        /// Description regarding the Purchase. (Optional)
        /// </summary>
        public string? Description { get => _description; set => _description = value?.Trim(); }


        private string? orderID;
        /// <summary>
        /// Generated by Merchant, used in reports. (Optional)
        /// </summary>
        public string? OrderID { get => orderID; set => orderID = value?.Trim(); }


        private string? mobile;
        /// <summary>
        /// If This Value if set, the user will see their registered card numbers in Zibal. (Optional)
        /// </summary>
        public string? Mobile { get => mobile; set => mobile = value?.Trim(); }


        private List<string>? _allowedCards;
        /// <summary>
        /// If you want to limit the user to pay only from certain card numbers, you can specify them here, each card in a separate string. (Optional)
        /// </summary>
        public List<string>? AllowedCards { get => _allowedCards; set => _allowedCards = value?.Select(x => x.Trim()).ToList(); }


        private string? ledgerID;
        /// <summary>
        /// LedgerID associated with this transaction. if the transaction is successful, the <see cref="Amount"/> will be added to this ledger. (Optional)
        /// </summary>
        public string? LedgerID { get => ledgerID; set => ledgerID = value?.Trim(); }

        /// <summary>
        /// The 10 digit long NationalCode, if set, the national code of the owner of the card used in payment is compared with this national code. 
        /// if the match fails, the transaction will be aborted. (Optional)
        /// </summary>
        public string? NationalCode { get; set; }

        /// <summary>
        /// Whether the card used in payment and the phone number should belong to the same person. (Optional)
        /// </summary>
        public bool CheckMobileWithCard { get; set; } = false;

    }

    /// <summary>
    /// The Placeholder for the response zibal sends in response to payment request.
    /// </summary>
    internal class CreateTransactionResponse
    {
        /// <summary>
        /// Generated By Zibal. Sent to Zibal in "Start" Phase.
        /// </summary>
        public int TrackID { get; set; }
        /// <summary>
        /// Response code regarding the request.
        /// </summary>
        public int Result { get; set; }
        /// <summary>
        /// Response Message provided by Zibal.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// The Placeholder for the request you send to Zibal to verify the transaction.
    /// </summary>
    internal class VerifyTransactionRequest
    {
        private string _merchant = string.Empty;

        /// <summary>
        /// provides access to the merchant. if this is a test, returns "zibal" else the merchant.
        /// </summary>
        public required string Merchant { get => IsTest ? "zibal" : _merchant; set => _merchant = value; }

        /// <summary>
        /// The TrackID provided by Zibal in the previous stage.
        /// </summary>
        public required long TrackID { get; set; }

        /// <summary>
        /// Is This a Test Transaction?
        /// </summary>
        [JsonIgnore]
        public bool IsTest { get; set; } = false;
    }

    /// <summary>
    /// The Placeholder for the response Zibal Sends in response to Payment Verification Request.
    /// </summary>
    internal class VerifyTransactionResponse
    {
        private DateTime paidAt;
        /// <summary>
        /// The exact time when the transaction happened.
        /// </summary>
        public DateTime PaidAt { get => paidAt; set => paidAt = value; }


        private string cardNumber = string.Empty;
        /// <summary>
        /// The Card Number used in the transaction (Masked). e.g: "62741****44"
        /// </summary>
        public string CardNumber { get => cardNumber; set => cardNumber = value; }

        /// <summary>
        /// The Transaction's Status. Check the codes at https://help.zibal.ir/IPG/API/#status-codes.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// The Amount Paid in Rials.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Reference Number of the transaction (in case the transaction was successful).
        /// </summary>
        public int? RefNumber { get; set; }

        /// <summary>
        /// Description of the transaction (in case the transaction was successful).
        /// </summary>
        public string? Description { get; set; }


        private string _orderID = string.Empty;
        /// <summary>
        /// Generated by Merchant, used in reports.
        /// </summary>
        public string OrderID { get => _orderID; set => _orderID = value.Trim(); }

        /// <summary>
        /// Response code regarding the request.
        /// </summary>
        public int Result { get; set; }
        /// <summary>
        /// Response Message provided by Zibal.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// The Placeholder for inquiry request, it's the same as <see cref="VerifyTransactionRequest"/>
    /// </summary>
    internal class InquiryTransactionRequest : VerifyTransactionRequest
    {

    }

    /// <summary>
    /// The Placeholder for inquiry response, it's almost the same as <see cref="VerifyTransactionResponse"/> but with a few extra fields.
    /// </summary>
    internal class InquiryTransactionResponse : VerifyTransactionResponse
    {
        /// <summary>
        /// The <see cref="DateTime"/> When The Transaction was Made.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The <see cref="DateTime"/> When The Transaction was Verified.
        /// </summary>
        public DateTime VerifiedAt { get; set; }

        /// <summary>
        /// 0 Means paid from transaction, 1 Means paid from Fee Wallet, 2 Means paid by the customer.
        /// </summary>
        public int Wage { get; set; }
    }

    /// <summary>
    /// The Placeholder to Deserialize Zibal's response to Payment creation if you're using the lazy method. 
    /// if you're NOT USING THE LAZY METHOD this information is provided via Query String to your callback address.
    /// </summary>
    internal class LazyCallbackResponse
    {
        private string _success = string.Empty;
        /// <summary>
        /// 1 if successful, 0 if fails.
        /// </summary>
        public string Success { get => _success; set => _success = value.Trim(); }

        /// <summary>
        /// Generated By Zibal.
        /// </summary>
        public int TrackID { get; set; }

        private string _orderID = string.Empty;
        /// <summary>
        /// Generated by Merchant, used in reports.
        /// </summary>
        public string OrderID { get => _orderID; set => _orderID = value.Trim(); }

        /// <summary>
        /// this is up to the merchant and doesn't have a value until the transaction is verified by the merchant.
        /// </summary>
        public int? Status { get; set; } = null;

        private string cardNumber = string.Empty;
        /// <summary>
        /// The card number used in the transaction.
        /// </summary>
        public string CardNumber { get => cardNumber; set => cardNumber = value.Trim(); }

        private string hashedCardNumber = string.Empty;
        /// <summary>
        /// Card number used in th transaction but Hashed.
        /// </summary>
        public string HashedCardNumber { get => hashedCardNumber; set => hashedCardNumber = value.Trim(); }
    }

    /// <summary>
    /// The Placeholder for required data in order to make a Transaction in Advanced mode.
    /// </summary>
    internal class CreateAdvancedTransactionRequest : CreateTransactionRequest
    {
        private int percentMode = 0;
        /// <summary>
        /// Specify if your multiplexing method is by percent. default to 0.
        /// </summary>
        public int PercentMode
        {
            get => percentMode;
            set
            {
                if (value == 1 || value == 0)

                    percentMode = value;
                else
                    throw new ArgumentException("Percent Mode must be 0 or 1");
            }
        }

        private int feeMode;
        /// <summary>
        /// The Method that the fee is paid in. 0 for paying from transaction, 1 for paying from the wallet connected, 2 for paid by the client. 
        /// </summary>
        public int FeeMode
        {
            get => feeMode;
            set
            {
                if (value == 0 || value == 1 || value == 2)
                    feeMode = value;
                else
                    throw new ArgumentException($"Fee Mode Value Must be 0 or 1 or 2");
            }
        }

        /// <summary>
        /// The List of beneficiaries in this transaction each as one Item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

    /// <summary>
    /// The Placeholder for the response Zibal Sends to you.
    /// </summary>
    internal class CreateAdvancedTransactionResponse : CreateTransactionResponse
    {
        /// <summary>
        /// information regarding the beneficiaries each in one item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

    /// <summary>
    /// The Placeholder for the response Zibal Sends to you.
    /// </summary>
    internal class VerifyAdvancedTransactionResponse : VerifyTransactionResponse
    {
        /// <summary>
        /// information regarding the beneficiaries each in one item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

    /// <summary>
    /// The Placeholder for the response Zibal Sends to you.
    /// </summary>
    internal class InquiryAdvancedTransactionResponse : InquiryTransactionResponse
    {
        /// <summary>
        /// information regarding the beneficiaries each in one item.
        /// </summary>
        public required List<MultiplexingInformation> MultiplexingInfos { get; set; }
    }

    /// <summary>
    /// The Placeholder to provide information for each of the multiplexing items.
    /// </summary>
    internal class MultiplexingInformation
    {
        private string? bankAccount;
        /// <summary>
        /// SHABA Number of the beneficiary.
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? BankAccount { get => bankAccount; set => bankAccount = value?.Trim(); }

        private string? subMerchantID;
        /// <summary>
        /// ID of the beneficiary.
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? SubMerchantID { get => subMerchantID; set => subMerchantID = value?.Trim(); }

        private string? walletID;
        /// <summary>
        /// Wallet ID. Not supported in "پرداختیاری".
        /// Specify Only One of these : BankAccount, SubMerchantID, WalletID
        /// </summary>
        public string? WalletID { get => walletID; set => walletID = value?.Trim(); }

        /// <summary>
        /// Amount or Percent.
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// Should the fee be paid by this item? effective only  when "FeeMode" is set to 0
        /// if not specified, the main beneficiary will be charged.
        /// </summary>
        public bool WagePayer { get; set; }
    }

}

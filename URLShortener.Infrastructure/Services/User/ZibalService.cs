using System.Text;
using System.Text.Json;
using URLShortener.Application.DTOs.ZibalDTOs;
using URLShortener.Application.Interfaces.Infrastructure.External;

namespace URLShortenerAPI.Services.User
{
    public class ZibalService : IZibalService
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

}

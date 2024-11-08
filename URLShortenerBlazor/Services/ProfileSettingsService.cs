using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Net;
using System.Text;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    public class ProfileSettingsService : IProfileSettingsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _authedHttpClient;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ProfileSettingsService(IHttpClientFactory httpClientFactory, HttpClient httpClient)
        {
            _httpClientFactory = httpClientFactory;
            _authedHttpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends the request to backend to update user's info in database.
        /// </summary>
        /// <param name="userUpdate">The Object Containing User's New Info.</param>
        /// <returns> a <see cref="UserDTO"/> Object showing the new Info. </returns>
        public async Task<APIResponse<UserDTO>> ChangeUserInfo(UserUpdateDTO userUpdate)
        {
            HttpRequestMessage req = new(HttpMethod.Put, "api/Users/UpdateUser")
            {
                Content = new StringContent(JsonSerializer.Serialize(userUpdate), Encoding.UTF8, "application/json")
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _authedHttpClient.SendAsync(req);
            APIResponse<UserDTO>? responseContent;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                responseContent = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
            {
                responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            }

            return responseContent!;
        }

        /// <summary>
        /// Request starting a change password sequence.
        /// </summary>
        /// <param name="identifier">The identifier used to Find the user from database.</param>
        /// <returns></returns>
        public async Task<APIResponse<string>> RequestChangePassword(string identifier)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/ResetPassword")
            {
                Content = new StringContent(JsonSerializer.Serialize(identifier), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<string>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;

        }

        /// <summary>
        /// Sends the Verification code sent to user's email and provided by user to backend.
        /// </summary>
        /// <param name="reqInfo"></param>
        /// <returns></returns>
        public async Task<APIResponse<UserDTO>> SendPasswordVerificationCode(CheckVerificationCode reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/CheckResetCode")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);
            APIResponse<UserDTO>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        /// <summary>
        /// After the user is verified, they can send the new passwords using this function.
        /// </summary>
        /// <param name="request">the object containing the new info.</param>
        /// <returns></returns>
        public async Task<APIResponse<UserDTO>> SendChangePassowrdRequest(ChangePasswordRequest request)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/ChangePassword")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDTO>? responseContent;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                responseContent = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
                responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        /// <summary>
        /// Starts the procedure of changing email. a code is sent to user's email if succesful.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async Task<APIResponse<string>> RequestChangingEmail(int userID)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/ResetEmail/{userID}");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _authedHttpClient.SendAsync(req);

            APIResponse<string>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        /// <summary>
        /// Sends the code provided by User to Backend.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="reqInfo"></param>
        /// <returns></returns>
        public async Task<APIResponse<string>> SendEmailVerificationCode(int userID, CheckVerificationCode reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/CheckEmailResetCode/{userID}")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage response = await _authedHttpClient.SendAsync(req);

            APIResponse<string>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        /// <summary>
        /// After the user is verified, they can send the new Email using this function.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="reqInfo"> the Object containing new information.</param>
        /// <returns></returns>
        public async Task<APIResponse<UserDTO>> SendChangeEmailRequest(int userID, ChangeEmailRequest reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/ChangeEmail/{userID}")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _authedHttpClient.SendAsync(req);

            APIResponse<UserDTO>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }
    }
}

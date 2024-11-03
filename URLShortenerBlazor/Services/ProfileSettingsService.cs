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
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ProfileSettingsService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
        }

        public async Task<APIResponse<UserDTO>> ChangeUserInfo(UserUpdateDTO userUpdate)
        {
            HttpRequestMessage req = new(HttpMethod.Put, "api/Users/UpdateUser")
            {
                Content = new StringContent(JsonSerializer.Serialize(userUpdate), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(req);
            APIResponse<UserDTO>? responseContent;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                responseContent = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
            {
                responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);
            }

            return responseContent!;
        }

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

        public async Task<APIResponse<UserDTO>> SendPasswordVerificationCode(CheckVerificationCode reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/CheckResetCode")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(req);
            APIResponse<UserDTO>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        public async Task<APIResponse<UserDTO>> SendChangePassowrdRequest(ChangePasswordRequest request)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "api/Users/ChangePassword")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDTO>? responseContent;

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                responseContent = new() { ErrorType = ErrorType.NotAuthorizedException };

            else
                responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        public async Task<APIResponse<string>> RequestChangingEmail(int userID)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/ResetEmail/{userID}");
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<string>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        public async Task<APIResponse<string>> SendEmailVerificationCode(int userID, CheckVerificationCode reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/CheckEmailResetCode/{userID}")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<string>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }

        public async Task<APIResponse<UserDTO>> SendChangeEmailRequest(int userID, ChangeEmailRequest reqInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, $"api/Users/ChangeEmail/{userID}")
            {
                Content = new StringContent(JsonSerializer.Serialize(reqInfo), Encoding.UTF8, "application/json")
            };
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            APIResponse<UserDTO>? responseContent;
            responseContent = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return responseContent!;
        }
    }
}

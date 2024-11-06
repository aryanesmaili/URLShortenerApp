using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using SharedDataModels.DTOs;
using SharedDataModels.Responses;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using URLShortenerBlazor.Services.Interfaces;

namespace URLShortenerBlazor.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _authedHttpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly CustomAuthProvider _stateProvider;

        public AuthenticationService(ILocalStorageService localStorage, IHttpClientFactory httpClientFactory, HttpClient httpClient, CustomAuthProvider stateProvider)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClient;
            _authedHttpClient = _httpClientFactory.CreateClient("Auth");
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
            _stateProvider = stateProvider;
        }

        /// <summary>
        /// initiates a login process, first it sends the user's inputs to backend, if they're correct, it fetches the CSRF tokens for this user.
        /// </summary>
        /// <param name="loginInfo"> a <see cref="UserLoginDTO"/> object containing the info required for login.</param>
        /// <returns>the response of API containing Info about the user.</returns>
        public async Task<APIResponse<UserDTO>> Login(UserLoginDTO loginInfo)
        {

            APIResponse<UserDTO> result = await SendUserCredentials(loginInfo);

            if (result.Success) // if the login was succesful
            {
                await FetchCSRFTokens(); // we fetch the csrf token.
            }
            return result!; // we return the response to show errors if any.
        }

        /// <summary>
        /// The Login process sends user's info to api using this function. it basically sends the object to backend and then deserializes the response.
        /// </summary>
        /// <param name="loginInfo"> a <see cref="UserLoginDTO"/> object containing the info required for login.</param>
        /// <returns>the response of API containing Info about the user.</returns>
        private async Task<APIResponse<UserDTO>> SendUserCredentials(UserLoginDTO loginInfo)
        {
            HttpRequestMessage req = new(HttpMethod.Post, "/api/Users/Login")
            {
                Content = new StringContent(JsonSerializer.Serialize(loginInfo), Encoding.UTF8, "application/json"),
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            // we send user's login info to server to see if it's correct.
            HttpResponseMessage response = await _httpClient.SendAsync(req);

            // then we desrialize the server's response.
            APIResponse<UserDTO>? result = await JsonSerializer.DeserializeAsync<APIResponse<UserDTO>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (response.IsSuccessStatusCode)
                await _localStorage.SetItemAsync("user", result!.Result); // we store the user data to local storage.

            return result!;
        }

        /// <summary>
        /// Fetches the CSRF tokens required for protecting against CSRF attacks. writes one of the tokens to localstorage.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if the fetching goes wrong.</exception>
        private async Task FetchCSRFTokens()
        {
            HttpRequestMessage req = new(HttpMethod.Get, "api/Users/antiforgery/token");
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            HttpResponseMessage tokenresponse = await _httpClient.SendAsync(req);

            APIResponse<string>? res = await JsonSerializer.DeserializeAsync<APIResponse<string>>(await tokenresponse.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            if (tokenresponse.IsSuccessStatusCode)
                await _localStorage.SetItemAsStringAsync("xsrf-token", res!.Result!); // the token that will be included in the header of the requests.
            else
                throw new Exception($"Failed Fetching CSRF Token : {res!.ErrorMessage}");
        }

        /// <summary>
        /// This function first invalidates the tokens in backend, then cleans localstorage.
        /// </summary>
        /// <returns></returns>
        public async Task LogOutAsync()
        {
            await _authedHttpClient.PostAsync("/api/Users/Logout", null); // Send null as there’s no content

            await ClearUserInfo(); // clear local storage 

            _stateProvider.MarkUserAsLoggedOut();
        }

        /// <summary>
        /// Sends user input to backend to register them.
        /// </summary>
        /// <param name="userCreateDTO">User's input.</param>
        /// <returns></returns>
        public async Task<APIResponse<UserDTO>> Register(UserCreateDTO userCreateDTO)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/api/Users/Register", userCreateDTO);

            APIResponse<UserDTO>? result = await response.Content.ReadFromJsonAsync<APIResponse<UserDTO>>();

            // returned in case of errors.
            return result!;
        }

        /// <summary>
        /// Gets the User's ID from local storage.
        /// </summary>
        /// <returns>User's ID</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<int> GetUserIDAsync()
        {
            int userID = (await _localStorage.GetItemAsync<UserDTO>("user") ?? throw new ArgumentNullException("User Data Does Not Exist")).ID;
            return userID;
        }

        /// <summary>
        /// Gets the User Info from localstorage.
        /// </summary>
        /// <returns>a <see cref="UserDTO"/> object containing user info.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<UserDTO> GetUserInfoAsync()
        {
            UserDTO userInfo = await _localStorage.GetItemAsync<UserDTO>("user") ?? throw new ArgumentNullException("User Data Does Not Exist");
            return userInfo;
        }

        /// <summary>
        /// updates the info stored in local storage.
        /// </summary>
        /// <param name="userInfo">new user info.</param>
        /// <returns></returns>
        public async Task UpdateUserInfo(UserDTO userInfo)
        {
            if (userInfo == null)
                return;
            await _localStorage.SetItemAsync("user", userInfo);
        }

        /// <summary>
        /// clears the userInfo from the local storage.
        /// </summary>
        /// <returns></returns>
        public async Task ClearUserInfo()
        {
            await _localStorage.RemoveItemAsync("xsrf-token"); // Remove local token 
            await _localStorage.RemoveItemAsync("user"); // remove user info
        }
    }
}

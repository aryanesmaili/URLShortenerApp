﻿using Blazored.LocalStorage;
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
            _authedHttpClient = _httpClientFactory.CreateClient("Auth");
            _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,  // Make property name matching case-insensitive
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Handle camelCase JSON property names
            };
            _stateProvider = stateProvider;
        }

        /// <summary>
        /// Sends the captcha token to backend to be verified there.
        /// </summary>
        /// <param name="token">the token to be sent to backend</param>
        /// <returns></returns>
        public async Task<APIResponse<CaptchaVerificationResponse>> VerifyCaptcha(string token)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/Users/Captcha", token);
            APIResponse<CaptchaVerificationResponse>? result = await JsonSerializer.DeserializeAsync<APIResponse<CaptchaVerificationResponse>>(await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions);

            return result!;
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
                await _stateProvider.FetchCSRFTokens(); // we fetch the csrf token.
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
        /// This function first invalidates the tokens in backend, then cleans localstorage.
        /// </summary>
        /// <returns></returns>
        public async Task LogOutAsync(bool backendLogout = true)
        {
            if (backendLogout)
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

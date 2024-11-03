using Microsoft.AspNetCore.Components.Authorization;
using URLShortenerBlazor.Services.Interfaces;
using SharedDataModels.DTOs;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace URLShortenerBlazor.Services
{
    public class CustomAuthProvider : AuthenticationStateProvider
    {
        private readonly IAuthenticationService _authenticationService;

        public CustomAuthProvider(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = await _authenticationService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");

            // Create claims principal with the identity created from JWT claims
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
    }
}

namespace WorkforceManager.Web.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using WorkforceManager.Models.Responses;
    using WorkforceManager.Services.Interfaces;

    public static class AuthenticationHelper
    {
        public static async Task<TokenResponseModel> GetElibilityTokenAsync(this IUserService userService, HttpClient client, string usernameInput, string passwordInput)
        {
            var baseAddress = @"https://localhost:5002/connect/token";

            var username = usernameInput;
            var password = passwordInput;
            var grantType = "password";
            var clientId = "workforcemanagerapp";
            var clientSecret = "secret";
            var form = new Dictionary<string, string>
                {
                    {"username", username},
                    {"password", password },
                    {"grant_type", grantType},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                };

            var tokenResponse = await client.PostAsync(baseAddress, new FormUrlEncodedContent(form));
            var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<TokenResponseModel>(jsonContent);

            if (token.AccessToken == null)
            {
                throw new UnauthorizedAccessException("Bad credentials, please try again!");
            }

            return token;
        }
    }
}

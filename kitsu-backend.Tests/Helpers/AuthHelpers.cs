using kitsu_backend.Features.Authentication;
using System.Net.Http.Json;

namespace kitsu_backend.Tests.Helpers
{
    public static class AuthHelpers
    {
        public const string TestPassword = "TestPassword123!";

        /// <summary>
        /// Registers a new user with a unique username and email through the API and returns the response, including the JWT.
        /// </summary>
        public static async Task<RegisterResponse> RegisterUserAsync(this HttpClient client)
        {
            var unique = Guid.NewGuid().ToString("N");
            var request = new RegisterRequest(
                Username: $"user{unique}",
                Email: $"{unique}@test.com",
                Password: TestPassword);

            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));

            return result;
        }

        public static async Task<string> GetAuthTokenAsync(this HttpClient client, string email)
        {
            var request = new LoginRequest(email, TestPassword);

            var response = await client.PostAsJsonAsync("/api/auth/login", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
            return result.Token;
        }
    }
}

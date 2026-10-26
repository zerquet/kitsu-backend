using kitsu.Features.MediaReactions;
using kitsu.Shared.Dtos;
using kitsu_backend.Entities;
using kitsu_backend.Features.UserAnime;
using kitsu_backend.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace kitsu_backend.Tests.Integration
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Prevent automatic redirection to the login page
            });
        }

        [Fact]
        public async Task CreateAnime_UnauthenticatedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/animes")
            {
                Content = new StringContent("{\"title\":\"Test Anime\"}", System.Text.Encoding.UTF8, "application/json")
            };

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateAnime_AuthenticatedNonAdminUser_ShouldReturnForbidden()
        {
            // Arrange
            var user = await _client.RegisterUserAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/animes")
            {
                Content = new StringContent("{\"title\":\"Test Anime\"}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateAnime_AdminUser_ShouldReturnCreated()
        {
            // Arrange
            // Create new user, then assign them the "Admin" role, and login to get updated token
            var nonAdminUser = await _client.RegisterUserAsync();
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await userManager.FindByEmailAsync(nonAdminUser.Email);
                if (user != null)
                    await userManager.AddToRoleAsync(user, "Admin");

            }
            var adminUserToken = await _client.GetAuthTokenAsync(nonAdminUser.Email);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/animes")
            {
                Content = new StringContent("{\"title\":\"Test Anime\", \"categories\":[]}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminUserToken);
            // Act
            var response = await _client.SendAsync(request);
            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMediaReaction_OnlyAuthorizedUserCanDeleteReaction()
        {
            // Arrange
            var user = await _client.RegisterUserAsync();
            var token = user.Token;
            var libraryEntryRequest = new HttpRequestMessage(HttpMethod.Post, "/api/library")
            {
                Content = new StringContent("{\"animeId\":1,\"watchStatus\":\"Watching\",\"episodesWatched\":10}", System.Text.Encoding.UTF8, "application/json")
            };
            libraryEntryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var libraryEntryResponse = await _client.SendAsync(libraryEntryRequest);
            libraryEntryResponse.EnsureSuccessStatusCode();
            var libraryEntry = await libraryEntryResponse.Content.ReadFromJsonAsync<GetLibraryResponse>();
            Assert.NotNull(libraryEntry);
            // Create a media reaction for the user
            var createReactionRequest = new HttpRequestMessage(HttpMethod.Post, "/api/mediaReactions")
            {
                Content = new StringContent($"{{\"animeId\":1,\"libraryEntryId\":{libraryEntry.LibraryEntryId},\"reaction\":\"Great anime!\"}}", System.Text.Encoding.UTF8, "application/json")
            };
            createReactionRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var createReactionResponse = await _client.SendAsync(createReactionRequest);
            createReactionResponse.EnsureSuccessStatusCode();
            var createdReaction = await createReactionResponse.Content.ReadFromJsonAsync<MediaReactionByUserResponse>();
            Assert.NotNull(createdReaction);

            // Act - Attempt to delete the reaction with a different user
            var anotherUser = await _client.RegisterUserAsync();
            var anotherToken = anotherUser.Token;
            var deleteRequestAnotherUser = new HttpRequestMessage(HttpMethod.Delete, $"/api/mediaReactions/{createdReaction.MediaReactionId}");
            deleteRequestAnotherUser.Headers.Authorization = new AuthenticationHeaderValue("Bearer", anotherToken);
            var deleteResponseAnotherUser = await _client.SendAsync(deleteRequestAnotherUser);

            // Assert - The deletion should fail with NotFound status
            Assert.Equal(HttpStatusCode.NotFound, deleteResponseAnotherUser.StatusCode);
        }
    }
}

using System.Net.Http.Json;
using System.Text.Json;

namespace kitsu_backend.Services
{
    /*Read-only client for Kitsu's public JSON:API (https://kitsu.io/api/edge). No API key needed.*/
    public interface IKitsuApiClient
    {
        Task<List<KitsuCategory>> GetCategoriesAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Returns Kitsu category ids per Kitsu anime id. Anime that Kitsu doesn't return (deleted, hidden) are left out.
        /// </summary>
        Task<Dictionary<int, List<int>>> GetAnimeCategoryIdsAsync(IReadOnlyCollection<int> kitsuAnimeIds, CancellationToken cancellationToken);
    }

    public record KitsuCategory(int Id, string Slug, int? ParentId);

    public class KitsuApiClient : IKitsuApiClient
    {
        public const string BaseUrl = "https://kitsu.io/api/edge/";
        public const int MaxAnimePerRequest = 20; // Kitsu's page size limit for anime
        private static readonly TimeSpan RequestDelay = TimeSpan.FromMilliseconds(250); // free, keyless API; don't hammer it

        private readonly HttpClient _http;
        public KitsuApiClient(HttpClient http) => _http = http;

        public async Task<List<KitsuCategory>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = new List<KitsuCategory>();
            string? url = "categories?page[limit]=500&include=parent&fields[categories]=slug,parent";

            while (url != null)
            {
                var document = await GetAsync(url, cancellationToken);
                foreach (var category in document.Data)
                {
                    categories.Add(new KitsuCategory(
                        int.Parse(category.Id),
                        category.Attributes?.Slug ?? "",
                        RelatedIds(category, "parent").Select(id => (int?)id).FirstOrDefault()));
                }
                url = document.Links?.Next;
            }

            return categories;
        }

        public async Task<Dictionary<int, List<int>>> GetAnimeCategoryIdsAsync(IReadOnlyCollection<int> kitsuAnimeIds, CancellationToken cancellationToken)
        {
            if (kitsuAnimeIds.Count > MaxAnimePerRequest)
                throw new ArgumentException($"Kitsu returns at most {MaxAnimePerRequest} anime per request.", nameof(kitsuAnimeIds));

            var url = $"anime?filter[id]={string.Join(',', kitsuAnimeIds)}&page[limit]={MaxAnimePerRequest}" +
                "&include=categories&fields[anime]=categories&fields[categories]=slug";
            var document = await GetAsync(url, cancellationToken);

            return document.Data.ToDictionary(anime => int.Parse(anime.Id), anime => RelatedIds(anime, "categories").ToList());
        }

        private async Task<JsonApiDocument> GetAsync(string url, CancellationToken cancellationToken)
        {
            await Task.Delay(RequestDelay, cancellationToken);
            var document = await _http.GetFromJsonAsync<JsonApiDocument>(url, cancellationToken);
            return document ?? throw new InvalidOperationException($"Kitsu returned an empty response for {url}");
        }

        private static IEnumerable<int> RelatedIds(JsonApiResource resource, string relationship)
        {
            if (resource.Relationships == null || !resource.Relationships.TryGetValue(relationship, out var related))
                return [];

            return related.Data.ValueKind switch
            {
                JsonValueKind.Object => [ParseId(related.Data)],
                JsonValueKind.Array => related.Data.EnumerateArray().Select(ParseId).ToList(),
                _ => [],
            };
        }

        private static int ParseId(JsonElement identifier) => int.Parse(identifier.GetProperty("id").GetString()!);

        // Only the parts of Kitsu's JSON:API responses that are read here
        private sealed class JsonApiDocument
        {
            public List<JsonApiResource> Data { get; set; } = [];
            public JsonApiLinks? Links { get; set; }
        }

        private sealed class JsonApiResource
        {
            public string Id { get; set; } = "";
            public JsonApiAttributes? Attributes { get; set; }
            public Dictionary<string, JsonApiRelationship>? Relationships { get; set; }
        }

        private sealed class JsonApiAttributes
        {
            public string? Slug { get; set; }
        }

        private sealed class JsonApiRelationship
        {
            public JsonElement Data { get; set; } // an object for to-one, an array for to-many, null when empty
        }

        private sealed class JsonApiLinks
        {
            public string? Next { get; set; }
        }
    }
}

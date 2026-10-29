using kitsu.Features.Admin;
using kitsu.Features.Animes;
using kitsu.Features.Franchises;
using kitsu.Features.MediaReactions;
using kitsu.Features.Users;
using kitsu_backend.Features.Admin;
using kitsu_backend.Features.Animes;
using kitsu_backend.Features.Authentication;
using kitsu_backend.Features.Categories;
using kitsu_backend.Features.Episodes;
using kitsu_backend.Features.Franchises;
using kitsu_backend.Features.UserAnime;

namespace kitsu_backend.Extensions
{
    public static class EndpointExtensions
    {
        /*Add Minimal API routes*/
        public static void AddEndpoints(WebApplication app)
        {
            //Admin
            CreateAnimeEndpoint.Map(app);
            UpdateAnimeEndpoint.Map (app);
            Features.Admin.GetAnimeByIdEndpoint.Map(app);
            GetAnimeEndpoint.Map(app);


            Features.Animes.GetAnimeByIdEndpoint.Map(app);
            GetAnimeEpisodesEndpoint.Map(app);
            GetAnimesByCategoryEndpoint.Map(app);
            GetAnimeByTitleEndpoint.Map(app);
            SearchAnimeEndpoint.Map(app);
            GetExploreAnimeEndpoint.Map(app);
            GetAnimesByCollectionEndpoint.Map(app);

            GetCategoriesEndpoint.Map(app);
            GetCategoryEndpoint.Map(app);

            AddToLibraryEndpoint.Map(app);
            RemoveFromLibraryEndpoint.Map(app);
            GetLibraryEndpoint.Map(app);
            GetLibraryEntryEndpoint.Map(app);
            GetProfileEndpoint.Map(app);
            UpdateLibraryEntryEndpoint.Map(app);
            UpdateProfileEndpoint.Map(app);
            AddFavoriteAnimeEndpoint.Map(app);
            RemoveFavoriteAnimeEndpoint.Map(app);
            GetFavoriteAnimesEndpoint.Map(app);
            UpdateFavoriteAnimesEndpoint.Map(app);
            AddMediaReactionEndpoint.Map(app);
            GetMediaReactionsByUserEndpoint.Map(app);
            GetMediaReactionByIdEndpoint.Map(app);
            UpdateMediaReactionEndpoint.Map(app);
            DeleteMediaReactionEndpoint.Map(app);

            CreateFranchiseEndpoint.Map(app);
            GetFranchisesEndpoint.Map(app);
            GetFranchisesByNameEndpoint.Map(app);
            GetRelatedAnimeEndpoint.Map(app);

            CreateEpisodeEndpoint.Map(app);
            GetEpisodesEndpoint.Map(app);

            LoginEndpoint.Map(app);
            RegisterEndpoint.Map(app);
        }
    }
}

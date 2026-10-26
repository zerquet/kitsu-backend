using kitsu.Features.Animes;
using kitsu.Features.Franchises;
using kitsu.Features.MediaReactions;
using kitsu.Features.Users;
using kitsu_backend.Features.Animes;
using kitsu_backend.Features.Authentication;
using kitsu_backend.Features.Categories;
using kitsu_backend.Features.Episodes;
using kitsu_backend.Features.Franchises;
using kitsu_backend.Features.UserAnime;

namespace kitsu.Extensions
{
    public static class HandlerExtensions
    {
        /*Add handlers (services)*/
        public static void AddHandlers(this IServiceCollection services)
        {
            //anime feature
            services.AddScoped<CreateAnimeHandler>();
            services.AddScoped<GetAnimeByIdHandler>();
            services.AddScoped<GetAnimeEpisodesHandler>();
            services.AddScoped<GetTopAnimeHandler>();
            services.AddScoped<GetAnimesByCategoryHandler>();
            services.AddScoped<GetAnimeByTitleHandler>();
            services.AddScoped<SearchAnimeHandler>();
            services.AddScoped<UpdateAnimeHandler>();
            services.AddScoped<GetExploreAnimeHandler>();
            services.AddScoped<GetAnimesByCollectionHandler>();

            //auth feature
            services.AddScoped<LoginHandler>();
            services.AddScoped<RegisterHandler>();

            //category feature
            services.AddScoped<GetCategoriesHandler>();
            services.AddScoped<GetCategoryHandler>();
            
            //episodes feature
            services.AddScoped<CreateEpisodeHandler>();
            services.AddScoped<GetEpisodesHandler>();

            //franchise feature
            services.AddScoped<CreateFranchiseHandler>();
            services.AddScoped<GetFranchisesHandler>();
            services.AddScoped<GetFranchisesByNameHandler>();
            services.AddScoped<GetRelatedAnimeHandler>();

            //user feature
            services.AddScoped<AddFavoriteAnimeHandler>();
            services.AddScoped<AddToLibraryHandler>();
            services.AddScoped<RemoveFavoriteAnimeHandler>();
            services.AddScoped<RemoveFromLibraryHandler>();
            services.AddScoped<GetFavoriteAnimesHandler>();
            services.AddScoped<GetLibraryHandler>();
            services.AddScoped<GetLibraryEntryHandler>();
            services.AddScoped<GetProfileHandler>();
            services.AddScoped<UpdateFavoriteAnimesHandler>();
            services.AddScoped<UpdateLibraryEntryHandler>();
            services.AddScoped<UpdateProfileHandler>();
            services.AddScoped<GetMediaReactionsByUserHandler>();
            services.AddScoped<GetMediaReactionByIdHandler>();
            services.AddScoped<AddMediaReactionHandler>();
            services.AddScoped<UpdateMediaReactionHandler>();
            services.AddScoped<DeleteMediaReactionHandler>();
        }
    }
}

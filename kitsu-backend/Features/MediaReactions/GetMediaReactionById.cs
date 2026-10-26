using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.MediaReactions;

public record GetMediaReactionByIdRequest(int Id);
public class GetMediaReactionByIdResponse : MediaReactionByUserResponse
{
    public ProfileResponse UserProfile { get; set; } = null!;
}

public class GetMediaReactionByIdHandler 
{
    private readonly ApplicationDbContext _context;

    public GetMediaReactionByIdHandler(ApplicationDbContext dbContext)
    {
        _context = dbContext;
    }
    public async Task<GetMediaReactionByIdResponse?> Handle(GetMediaReactionByIdRequest request, CancellationToken cancellationToken)
    {
        var reaction = await (from r in _context.MediaReactions
                              join a in _context.Animes on r.AnimeId equals a.Id
                              join u in _context.Users on r.ApplicationUserId equals u.Id
                              where r.Id == request.Id
                              select new GetMediaReactionByIdResponse
                              {
                                  MediaReactionId = r.Id,
                                  UserId = r.ApplicationUserId,
                                  Reaction = r.Reaction,
                                  UpvoteCount = r.UpvoteCount,
                                  AnimeId = r.AnimeId,
                                  Anime = new Anime
                                  {
                                      Id = a.Id,
                                      Title = a.Title,
                                      Year = a.Year,
                                      MediaType = a.MediaType,
                                      ImageUrl = a.ImageUrl
                                  },
                                  LibraryEntryId = r.LibraryEntryId,
                                  UserProfile = new ProfileResponse
                                  {
                                      Id = u.Id,
                                      DisplayId = u.DisplayId,
                                      Username = u.UserName,
                                      Bio = u.Bio,
                                      Location = u.Location,
                                      Birthday = u.Birthday,
                                      Gender = u.Gender,
                                      ProfilePhotoId = u.ProfilePhotoId,
                                      CoverPhotoId = u.CoverPhotoId
                                  }
                              }).FirstOrDefaultAsync(cancellationToken);
        
        return reaction;
    }
}

public static class GetMediaReactionByIdEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/mediaReactions/{id:int}", async (
            [FromRoute] int id, 
            GetMediaReactionByIdHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetMediaReactionByIdRequest(id);
            
            var result = await handler.Handle(query, ct);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        });
    }
}
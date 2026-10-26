using kitsu_backend.Database;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Categories;
/// <summary>
/// Request to get a list of categories that are in the database.
/// </summary>
public record GetCategoriesRequest();
public record CategoryBriefResponse(int Id, string? Name, string? Slug);

public class GetCategoriesHandler
{
    private readonly ApplicationDbContext _context;
    public GetCategoriesHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<CategoryBriefResponse>> Handle(GetCategoriesRequest request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Select(c => new CategoryBriefResponse(c.Id, c.Name, c.Slug))
            .ToListAsync(cancellationToken: cancellationToken);

        return categories;
    }
}

public static class GetCategoriesEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
            GetCategoriesHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetCategoriesRequest(), ct);
            
            return Results.Ok(result);
        });
    }
}

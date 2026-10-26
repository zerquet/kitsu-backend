using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Categories;
/// <summary>
/// Request to get brief information about a category for the explore page
/// </summary>
/// <param name="Title"></param>
public record GetCategoryRequest(string Title);
public record CategoryResponse(int Id, string? Name, string? Description, string? Slug);

public class GetCategoryHandler
{
    private readonly ApplicationDbContext _context;

    public GetCategoryHandler(ApplicationDbContext context) => _context = context;

    public async Task<CategoryResponse?> Handle(GetCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Where(c => c.Slug == request.Title)
            .Select(a => new CategoryResponse(a.Id, a.Name, a.Description, a.Slug))
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return category;
    }
}

public static class GetCategoryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/{title}", async (
            [FromRoute] string title, 
            GetCategoryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetCategoryRequest(title);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });

    }
}
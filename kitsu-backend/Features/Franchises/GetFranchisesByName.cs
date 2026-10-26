using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Franchises;
/// <summary>
/// Admin only. Request to get a list of franchises given a name. Used when creating an anime record.
/// </summary>
/// <param name="Name"></param>
public record GetFranchisesByNameRequest(string Name);
public record FranchiseSearchResultResponse(int Id, string? Name);

public class GetFranchisesByNameHandler
{
    private readonly ApplicationDbContext _context;
    public GetFranchisesByNameHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<FranchiseSearchResultResponse>> Handle(GetFranchisesByNameRequest request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(request.Name))
            return [];
        var franchises = await _context.Franchises
            .Where(f => f.Name.Contains(request.Name))
            .Take(4)
            .Select(franchise => new FranchiseSearchResultResponse(franchise.Id, franchise.Name))
            .ToListAsync(cancellationToken: cancellationToken);

        return franchises;
    }
}

public static class GetFranchisesByNameEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/franchises/byName", async (
            [FromQuery] string name, 
            GetFranchisesByNameHandler handler, 
            CancellationToken ct) =>
        {
            var query = new GetFranchisesByNameRequest(name);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}

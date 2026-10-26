using kitsu_backend.Database;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Franchises;
/// <summary>
/// Request to get all franchises in the database.
/// </summary>
public record GetFranchisesRequest();
public record FranchiseResponse(int Id, string? Name);

public class GetFranchisesHandler
{
    private readonly ApplicationDbContext _context;
    public GetFranchisesHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<FranchiseResponse>> Handle(GetFranchisesRequest request, CancellationToken cancellationToken)
    {
        var franchises = await _context.Franchises
            .Select(f => new FranchiseResponse(f.Id, f.Name))
            .ToListAsync(cancellationToken);

        return franchises;
    }
}

public static class GetFranchisesEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/franchises", async (
            GetFranchisesHandler handler, 
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetFranchisesRequest(), ct);

            return Results.Ok(result);
        });
    }
}

using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kitsu_backend.Features.Franchises;
/// <summary>
/// Admin only. Request to create an anime franchise.
/// </summary>
/// <param name="Name"></param>
public record CreateFranchiseRequest(string Name);

public class CreateFranchiseHandler
{
    private readonly ApplicationDbContext _context;
    public CreateFranchiseHandler(ApplicationDbContext context) => _context = context;
    public async Task Handle(CreateFranchiseRequest request, CancellationToken cancellationToken)
    {
        var franchise = new Franchise { Name = request.Name };
        
        await _context.Franchises.AddAsync(franchise, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);

    }
}

public static class CreateFranchiseEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/franchises", [Authorize(Roles = "Admin")] async (
            [FromBody] CreateFranchiseRequest request, 
            CreateFranchiseHandler handler, 
            CancellationToken ct) =>
        {
            await handler.Handle(request, ct);

            return Results.Created();
        });
    }
}
using kitsu_backend.Database;
using kitsu_backend.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Common;

namespace kitsu_backend.Tests.Integration
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Not "Development", so user secrets (real connection strings and keys) are never loaded in tests
            builder.UseEnvironment("Testing");

            // Config the app reads at startup. Supplied here so tests also run on machines without user secrets (e.g. CI)
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JWT:SigningKey"] = new string('k', 64), // HS512 needs a long key
                    ["JWT:Issuer"] = "test-issuer",
                    ["JWT:Audience"] = "test-audience",
                }));

            builder.ConfigureServices(services =>
            {
                // Remove the existing ApplicationDbContext registration and replace it with an SQLite database for testing
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (dbContextDescriptor != null)
                    services.Remove(dbContextDescriptor);

                var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

                if (dbConnectionDescriptor != null)
                    services.Remove(dbConnectionDescriptor);

                // A single open connection keeps the in-memory database alive for the lifetime of the host
                services.AddSingleton<DbConnection>(provider =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();
                    return connection;
                });

                services.AddDbContext<ApplicationDbContext>((provider, options) =>
                {
                    var connection = provider.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });
            });
        }

        // Create the schema and seed roles once, when the host is built, instead of in every test
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Safety check: never seed against anything but the SQLite test database
            if (db.Database.ProviderName?.Contains("Sqlite") != true)
                throw new InvalidOperationException($"Integration tests must use SQLite but the provider is '{db.Database.ProviderName}'.");

            db.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            TestDbSeeder.SeedRoles(roleManager);
            TestDbSeeder.SeedAnime(db);

            return host;
        }
    }
}

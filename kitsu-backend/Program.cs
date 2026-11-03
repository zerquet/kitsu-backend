using kitsu.Extensions;
using kitsu_backend.Database;
using kitsu_backend.Extensions;
using kitsu_backend.Services;
using kitsu_backend.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
/*Configure Swagger. Add security definition to add option to authenticate using JWT token for easier testing.*/
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new() { Title = "Anime Tracker", Version = "v1" });
  c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    In = ParameterLocation.Header,
    Description = "Please enter a valid token",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "Bearer"
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
builder.Services.AddCors(options =>
{
  options.AddPolicy("local-frontend", policy =>
  {
    //Make sure you use an HTTPS profile or else you get a CORS error. Self-sign cert
    //for localhost
    policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:5173", 
            "https://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod();
  });
  options.AddPolicy("prod-frontend", policy =>
  {
      policy.WithOrigins("https://icy-sky-0b3ce1210.6.azurestaticapps.net", "http://icy-sky-0b3ce1210.6.azurestaticapps.net")
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
  options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
});

if(builder.Environment.IsDevelopment())
{
    //Connection string is stored using the Secret Manager tool. See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration["ConnectionStrings:localDb"]));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        // Azure SQL serverless pauses when idle; retry so the first request waits for it to resume instead of returning a 500
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("azureProdDb"),
            sql => sql.EnableRetryOnFailure(
                maxRetryCount: 8,
                maxRetryDelay: TimeSpan.FromSeconds(15),
                errorNumbersToAdd: null)));
}

AzureStorageService.Initialize(builder.Configuration, builder.Environment);


builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddHttpClient<IKitsuApiClient, KitsuApiClient>(client =>
{
    client.BaseAddress = new Uri(KitsuApiClient.BaseUrl);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.api+json");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("kitsu-backend");
});

HandlerExtensions.AddHandlers(builder.Services);

builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddAntiforgery();

var app = builder.Build();

EndpointExtensions.AddEndpoints(app);

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("local-frontend");
app.UseCors("prod-frontend");

app.UseAuthentication();

app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();

app.Run();

public partial class Program { } //Make the Program class public for integration testing
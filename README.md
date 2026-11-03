# Kitsu Backend

REST API for an anime tracker based on [Kitsu](https://kitsu.app). Users can browse anime, build a personal library with watch progress and ratings, add favorites, and post reactions. Admins manage the anime catalog.

Frontend: https://github.com/zerquet/kitsu-frontend · Live demo: https://icy-sky-0b3ce1210.6.azurestaticapps.net/explore

## Tech stack

- **.NET 8**, ASP.NET Core Minimal APIs
- **EF Core 8** with SQL Server and code-first migrations
- **ASP.NET Core Identity** with **JWT bearer** authentication and role-based authorization
- **Azure Blob Storage** for anime and user photos
- **Swagger / OpenAPI** with a Bearer auth button for quick auth testing
- Hosted on **Azure App Service** and **Azure SQL**

## Architecture

The code is structured as vertical slices. Each feature lives in one file under `Features/<Area>/` that holds its request/response types, handler and endpoint mapping, so a change to one feature touches one file.

```
Database/     ApplicationDbContext
Entities/     EF Core entities (Anime, AnimeCategory, Category, Franchise, Episode, LibraryEntry, FavoriteAnime, MediaReaction, ApplicationUser)
Features/     One file per use case, grouped by area (Admin, Animes, Auth, Categories, Episodes, Franchises, MediaReactions, UserAnime, Users)
Extensions/   Endpoint, handler and identity registration
Services/     JWT token creation, Kitsu API client
Shared/       DTOs, constants, blob storage helper
Migrations/   EF Core migrations
```

Tests live in `kitsu-backend.Tests/`: unit tests for handler logic and integration tests that run the API against in-memory SQLite.

## Authorization model

| Access | Who | Examples |
| --- | --- | --- |
| Public | Anyone | Browse/search anime, categories, franchises, view any user's profile, library, favorites and reactions |
| Authenticated | Any logged-in user | Edit own library, favorites, reactions and profile |
| Admin | Users in the `Admin` role | Search, create and update anime, create episodes and franchises, sync anime categories from Kitsu |

- The user identity for write operations always comes from the JWT, never from the request body, so users can only modify their own data.
- Registration always assigns the `User` role. The `Admin` role can only be granted directly in the database.
- Tokens expire after 7 days. Role changes take effect at the next login.

## Endpoints

| Method | Route | Auth |
| --- | --- | --- |
| POST | `/api/auth/register` | - |
| POST | `/api/auth/login` | - |
| GET | `/api/animes/explore` (highest rated, new and popular) | - |
| GET | `/api/animes/explore/{collection}` (`highest-rated`, `new`, `most-popular`) | - |
| GET | `/api/animes/{id}` | - |
| GET | `/api/animes/search` | - |
| GET | `/api/animes/byTitle` | - |
| GET | `/api/{category}/animes` | - |
| GET | `/api/anime/{animeId}/episodes` | - |
| GET | `/api/animes/{animeId}/media-reactions` | - |
| GET | `/api/admin/animes/search`, `/api/admin/animes/{id}` | Admin |
| POST / PUT | `/api/admin/animes` | Admin |
| POST | `/api/admin/categories/sync-from-kitsu` | Admin |
| GET | `/api/categories`, `/api/categories/{title}` | - |
| GET | `/api/episodes` | - |
| POST | `/api/episodes` | Admin |
| GET | `/api/franchises`, `/api/franchises/byName`, `/api/franchises/{id}/animes` | - |
| POST | `/api/franchises` | Admin |
| GET | `/api/users/{userId}` | - |
| PUT | `/api/users` | User |
| GET | `/api/library/{userId}` | - |
| GET | `/api/libraryEntry/{animeId}` | User |
| POST / PUT | `/api/library` | User |
| DELETE | `/api/library/{animeId}` | User |
| GET | `/api/{userId}/favorites` | - |
| POST / DELETE | `/api/favorites/{animeId}` | User |
| PUT | `/api/favorites/updatebulk` | User |
| GET | `/api/{userId}/mediaReactions`, `/api/mediaReactions/{id}` | - |
| POST / PUT | `/api/mediaReactions` | User |
| DELETE | `/api/mediaReactions/{id}` | User |

Full request and response shapes are in Swagger at `/swagger`.

## Notable behavior

- Library updates are validated server-side: episode progress is clamped to the anime's episode count, and the watch status is corrected to match progress (for example, finishing every episode sets `completed`).
- User ratings must be between 1 and 4. An out-of-range rating is ignored and the previous rating is kept.
- Media reactions are returned with the user's library payload.
- The explore endpoints take an optional `category` query parameter (a category slug) to filter results.
- The Kitsu category sync links local anime to categories using Kitsu's own tags, matched by `KitsuId` and category slug. It only adds missing links, so it is safe to re-run. Pass `?dryRun=true` to see what would change without saving.

## Running locally

Prerequisites: .NET 8 SDK and a SQL Server instance (LocalDB, Express or Docker).

1. Clone the repo and go to the project folder:

   ```bash
   git clone https://github.com/zerquet/kitsu-backend.git
   cd kitsu-backend/kitsu-backend
   ```

2. Store secrets with the .NET Secret Manager. Nothing sensitive is committed.

   ```bash
   dotnet user-secrets set "ConnectionStrings:localDb" "<your SQL Server connection string>"
   dotnet user-secrets set "ConnectionStrings:AzureStorage" "<your Azure Storage connection string>"
   dotnet user-secrets set "JWT:SigningKey" "<random string, at least 64 characters>"
   ```

3. Apply migrations and run:

   ```bash
   dotnet tool restore
   dotnet ef database update
   dotnet run --launch-profile https
   ```

4. Open `https://localhost:7017/swagger`. Register a user, log in, then click **Authorize** and paste the token.

To make a local admin, insert a row in `AspNetUserRoles` linking your user to the `Admin` role in `AspNetRoles`.

The CORS policy allows the frontend on `localhost:4200` and `localhost:5173`. Use the HTTPS profile.

To run the tests, from the repo root:

```bash
dotnet test
```

## Production configuration

Production secrets are set as Azure App Service settings, not files:

| Setting | Type |
| --- | --- |
| `azureProdDb` | Connection string (SQLAzure) |
| `AzureStorage` | Connection string |
| `JWT__SigningKey` | App setting |

## Roadmap

- Request validation and a global ProblemDetails error handler
- Pagination on list endpoints
- Rate limiting on auth endpoints

# TaskMarketplace — Refactored (Controllers + Clean-ish Layers)

This is a refactor of your minimal API into a layered, controller-based ASP.NET Core Web API solution.

## Structure

```
TaskMarketplace-Refactor/
├─ TaskMarketplace.WebApi/                # Web API host (Program, Controllers, Swagger)
│  ├─ Controllers/
│  └─ appsettings.json
├─ TaskMarketplace.Service/               # Service implementations (business logic)
├─ TaskMarketplace.Service.Abstractions/  # Service interfaces
├─ TaskMarketplace.DAL/                   # EF Core DbContext + table models (+ Migrations)
│  └─ Models/
├─ TaskMarketplace.Contracts/             # DTOs (Requests/Responses)
└─ TaskMarketplace.Tools/                 # Helpers (no dependencies on other projects)
```

> **Note:** I did not generate a `.sln` file here. Easiest is to create one locally and add these projects:
>
> ```bash
> dotnet new sln -n TaskMarketplace
> 
> dotnet new webapi -n TaskMarketplace.WebApi --no-https
> dotnet new classlib -n TaskMarketplace.Service
> dotnet new classlib -n TaskMarketplace.Service.Abstractions
> dotnet new classlib -n TaskMarketplace.DAL
> dotnet new classlib -n TaskMarketplace.Contracts
> dotnet new classlib -n TaskMarketplace.Tools
> 
> # Replace the generated files with the ones from this archive (keep the .csproj names)
> 
> dotnet sln add TaskMarketplace.WebApi/TaskMarketplace.WebApi.csproj
> dotnet sln add TaskMarketplace.Service/TaskMarketplace.Service.csproj
> dotnet sln add TaskMarketplace.Service.Abstractions/TaskMarketplace.Service.Abstractions.csproj
> dotnet sln add TaskMarketplace.DAL/TaskMarketplace.DAL.csproj
> dotnet sln add TaskMarketplace.Contracts/TaskMarketplace.Contracts.csproj
> dotnet sln add TaskMarketplace.Tools/TaskMarketplace.Tools.csproj
> 
> # Project references
> dotnet add TaskMarketplace.WebApi reference TaskMarketplace.Service
> dotnet add TaskMarketplace.WebApi reference TaskMarketplace.Service.Abstractions
> dotnet add TaskMarketplace.WebApi reference TaskMarketplace.DAL
> dotnet add TaskMarketplace.WebApi reference TaskMarketplace.Contracts
> dotnet add TaskMarketplace.WebApi reference TaskMarketplace.Tools
> 
> dotnet add TaskMarketplace.Service reference TaskMarketplace.Service.Abstractions
> dotnet add TaskMarketplace.Service reference TaskMarketplace.DAL
> dotnet add TaskMarketplace.Service reference TaskMarketplace.Contracts
> dotnet add TaskMarketplace.Service reference TaskMarketplace.Tools
> 
> # NuGet packages
> dotnet add TaskMarketplace.WebApi package Microsoft.AspNetCore.Authentication.JwtBearer
> dotnet add TaskMarketplace.WebApi package Swashbuckle.AspNetCore
> dotnet add TaskMarketplace.WebApi package Microsoft.EntityFrameworkCore
> dotnet add TaskMarketplace.WebApi package Npgsql.EntityFrameworkCore.PostgreSQL
> dotnet add TaskMarketplace.WebApi package Microsoft.EntityFrameworkCore.Design
> dotnet add TaskMarketplace.DAL package Microsoft.EntityFrameworkCore
> dotnet add TaskMarketplace.DAL package Npgsql.EntityFrameworkCore.PostgreSQL
> dotnet add TaskMarketplace.Service package Microsoft.Extensions.Configuration.Abstractions
> dotnet add TaskMarketplace.Service package System.IdentityModel.Tokens.Jwt
> 
> # Run EF migrations from WebApi (recommended) or DAL (choose one project as startup)

> # Example (from WebApi):
> dotnet ef migrations add Initial --project TaskMarketplace.DAL --startup-project TaskMarketplace.WebApi
> dotnet ef database update --project TaskMarketplace.DAL --startup-project TaskMarketplace.WebApi
> 
> # Run
> dotnet run --project TaskMarketplace.WebApi
> ```

### JWT in Swagger

Swagger is configured with a Bearer schema. Press **Authorize** and paste:
```
Bearer <your-token>
```

## Notes
- `Tools` contains a simple `PasswordHasher` (SHA256). For production, replace with ASP.NET Core Identity or a modern password hasher (e.g., PBKDF2/Argon2).
- DTOs are used instead of exposing EF entities.
- Authorization policies: `AdminOnly` and `UserOnly`.

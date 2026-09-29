# Dawaya.Api: slice 1 (profiles + catalog)

## Setup
```bash
dotnet new webapi -n Dawaya.Api --use-controllers
cd Dawaya.Api
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```
Copy the `Domain/` and `Data/` folders into the project.

## Program.cs (add before `var app = builder.Build();`)
```csharp
using Dawaya.Api.Data;
using Dawaya.Api.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();
```

## appsettings.json
```json
"ConnectionStrings": {
  "Default": "Server=(localdb)\\MSSQLLocalDB;Database=Dawaya;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## First migration
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCatalogAndProfiles
dotnet ef database update
```

## Notes
- Enums are stored as strings, so the data stays readable in SQL Server.
- Enforced in the database: one "myself" profile per user, ingredient pair order (A < B) and uniqueness, non-null barcode uniqueness, and one product per (medicine, form, strength).
- CreatedAt/UpdatedAt are stamped in `SaveChanges`.
- Not in this slice: the audit interceptor (needs the admin slice), scanning, prescriptions, scheduling.

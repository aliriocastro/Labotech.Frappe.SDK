# Labotech.Frappe.SDK

.NET SDK for [Frappe Framework](https://frappeframework.com/) / ERPNext — core interfaces, HTTP connector, and data access layer in a single repository, published as three independent NuGet packages.

## Packages

| Package | Description |
|---|---|
| `Labotech.Frappe.Connector.Core` | Base interfaces (`IFrappeBaseEntity`, `IFrappeEntity`, `IFrappeChildEntity`) and shared JSON settings |
| `Labotech.Frappe.Connector` | HTTP client, service layer, and fluent query builder for Frappe REST APIs |
| `Labotech.Frappe.Data` | Repository pattern data access with linq2db for direct database queries |

### Dependency graph

```
Labotech.Frappe.Connector.Core    (no internal dependencies)
        ↑
Labotech.Frappe.Connector         (depends on Core)
        ↑
Labotech.Frappe.Data              (depends on Connector → Core)
```

Install only the package(s) you need — dependencies are pulled in transitively.

## Installation

### 1. Create a GitHub Personal Access Token (PAT)

Go to [GitHub Settings → Tokens (classic)](https://github.com/settings/tokens) and create a token with the **`read:packages`** scope.

### 2. Add `nuget.config` to your solution root

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/aliriocastro/index.json" />
  </packageSources>
</configuration>
```

### 3. Authenticate locally (once per machine)

```bash
dotnet nuget update source github \
  --username aliriocastro \
  --password YOUR_GITHUB_PAT \
  --store-password-in-clear-text \
  --configfile nuget.config
```

> **Important:** Do not commit the PAT to source control.

### 4. Add the package reference

```xml
<!-- Pick the package(s) you need -->
<PackageReference Include="Labotech.Frappe.Connector.Core" Version="1.2.8" />
<PackageReference Include="Labotech.Frappe.Connector" Version="1.2.8" />
<PackageReference Include="Labotech.Frappe.Data" Version="1.2.8" />
```

---

## Labotech.Frappe.Connector.Core

Core interfaces, base entities, and shared contracts — the foundation of the SDK.

| Type | Description |
|---|---|
| `IFrappeBaseEntity` | Base interface — exposes `Name` (document ID) and `Doctype` |
| `IFrappeEntity` | Extends base with `DocStatus`, `Creation`, `Modified`, `Owner`, `Idx` |
| `IFrappeChildEntity` | Extends entity with `Parent`, `ParentField`, `ParentType` for child tables |
| `ERPNextJsonSerializationSettings` | Shared `System.Text.Json` serialization configuration |
| `FrappeEntityExtensions` | Extension methods for entity JSON serialization/deserialization |

---

## Labotech.Frappe.Connector

HTTP connector for Frappe REST APIs with a typed service layer and fluent query builder.

### Services & Contracts

| Interface | Implementation | Description |
|---|---|---|
| `IFrappeService` | `FrappeService` | High-level CRUD: Get, Insert, Update, Delete, Rename, BulkUpdate, permissions |
| `IFrappeHttpClient` | `FrappeHttpClient` | Low-level HTTP client for resource/method endpoints |
| `IFrappeQueryFluent<T>` | `FrappeQueryFluent<T>` | Fluent query builder with chainable filters, ordering, pagination |

### Core Types

| Type | Description |
|---|---|
| `FrappeDocPermission` | Enum: `read`, `write`, `create`, `delete`, `submit`, `cancel`, `amend` |
| `FrappeDocStatus` | Enum: Draft (0), Submitted (1), Cancelled (2) |
| `FrappeFilterOperator` | Enum: `=`, `!=`, `like`, `not like`, `in`, `not in`, `between`, `>`, `<`, `>=`, `<=`, `is` |
| `FrappeHttpRequestException` | Custom exception with HTTP status and Frappe error details |
| `HtmlEscapingConverter` | `JsonConverter<string>` — prevents HTML escaping in JSON serialization |

### Usage

```csharp
// Register services (DI)
services.AddHttpClient<IFrappeHttpClient, FrappeHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://your-site.frappe.cloud");
    client.DefaultRequestHeaders.Add("Authorization", "token api_key:api_secret");
});
services.AddScoped<IFrappeService, FrappeService>();

// CRUD operations
var customer = await frappeService.GetDocByNameAsync<Customer>("Customer", "CUST-001", null);
var newCustomer = await frappeService.InsertAsync(customer);
await frappeService.DeleteDocAsync("Customer", "CUST-001");

// Fluent query builder
var results = await query
    .From("Sales Invoice")
    .Select("name", "customer", "grand_total")
    .Where("docstatus", FrappeFilterOperator.Equals, "1")
    .Where("grand_total", FrappeFilterOperator.GreaterThan, "1000")
    .OrderBy("creation desc")
    .Limit(50)
    .ExecuteAsync<SalesInvoice>();
```

---

## Labotech.Frappe.Data

Repository pattern combining Frappe REST API operations with direct database access via [linq2db](https://linq2db.github.io/).

| Method | Backend | Description |
|---|---|---|
| `GetByIdAsync(name)` | linq2db | Get a single document by its `Name` |
| `GetAllAsync(query)` | linq2db | Query with `IQueryable<T>` lambda |
| `InsertAsync(entity)` | Frappe API | Insert via REST API (triggers server-side hooks) |
| `InsertOnDatabaseAsync(entity)` | linq2db | Insert directly to DB (bypasses Frappe hooks) |
| `InsertManyOnDatabaseAsync(entities)` | linq2db | Bulk insert directly to DB |
| `UpdateAsync(entity)` | Frappe API | Update via REST API |
| `DeleteAsync(entity)` | Frappe API | Delete via REST API |
| `Table` | linq2db | Raw `IQueryable<TEntity>` for custom LINQ queries |

### Usage

```csharp
// Register services
services.AddHttpClient<IFrappeHttpClient, FrappeHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://your-site.frappe.cloud");
    client.DefaultRequestHeaders.Add("Authorization", "token api_key:api_secret");
});
services.AddScoped<IFrappeService, FrappeService>();
services.AddLinqToDBContext<FrappeDataConnection>((provider, options) =>
    options.UseMariaDB("Server=...;Database=...;User=...;Password=..."));
services.AddScoped<IFrappeRepository<Customer>, FrappeRepository<Customer>>();

// CRUD via repository
var customer = await repo.GetByIdAsync("CUST-001");
await repo.InsertAsync(newCustomer);
await repo.UpdateAsync(customer);
await repo.DeleteByNameAsync("CUST-001");

// Direct database queries
var highValue = await repo.GetAllAsync(q =>
    q.Where(i => i.GrandTotal > 10000)
     .OrderByDescending(i => i.Creation)
     .Take(50));

// Bulk insert (bypasses Frappe hooks)
await repo.InsertManyOnDatabaseAsync(thousandsOfRecords);
```

---

## Target Framework

`netstandard2.1` (all packages)

## Development

```bash
git clone https://github.com/aliriocastro/Labotech.Frappe.SDK.git
cd Labotech.Frappe.SDK
dotnet restore
dotnet build
```

## Publishing a new version

Versioning is controlled by **git tags**. A single tag publishes all three packages at the same version.

```bash
# 1. Commit your changes
git add -A && git commit -m "feat: add retry policy"

# 2. Push to trigger CI
git push

# 3. Tag with the new version
git tag v1.3.0

# 4. Push the tag — publishes all 3 NuGet packages
git push origin v1.3.0
```

| Event | Workflow | Steps |
|---|---|---|
| Push / PR | `ci.yml` | `dotnet restore` → `dotnet build -c Release` |
| Tag `v*` | `publish.yml` | Extract version → `dotnet pack` (3 packages) → push to GitHub Packages |

## Secrets

| Secret | Purpose |
|---|---|
| `GITHUB_TOKEN` | Push packages (auto-provided by Actions) |

No `PACKAGES_TOKEN` needed — all internal dependencies use `ProjectReference` at build time.

## License

MIT

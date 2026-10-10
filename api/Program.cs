using ListingsApi.Data;
using ListingsApi.Endpoints;
using ListingsApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// --- База: SQLite-файл. Путь считаем от папки приложения, а не от текущей папки терминала ---
var rawConnection = builder.Configuration.GetConnectionString("Default") ?? "Data Source=App_Data/listings.db";
var csb = new SqliteConnectionStringBuilder(rawConnection);
csb.DataSource = Path.GetFullPath(csb.DataSource, builder.Environment.ContentRootPath);
Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlite(csb.ToString())
    // EF Core 9+ по умолчанию падает при Migrate(), если снимок модели чуть отличается от кода.
    // Вместо падения при старте пишем предупреждение в лог (проверить: dotnet ef migrations has-pending-model-changes).
    .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<DistrictService>();

builder.Services.AddOpenApi();

// CORS нужен только при разработке (Angular на :4200, API на :5000).
// На сервере фронтенд и API отдаются с одного адреса — CORS не задействован.
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- Создание/обновление базы при старте ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DatabaseBootstrap.Apply(db);
}

app.UseCors();

// --- Фронтенд (Angular собирается в wwwroot) ---
app.UseDefaultFiles();
app.UseStaticFiles();

// --- Swagger: в разработке всегда, на сервере — только если Swagger:Enabled=true ---
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Listings API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapListingEndpoints();
app.MapDistrictEndpoints();

app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();                      // разрешение для фронтенда

var app = builder.Build();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

// временные данные — на неделе 3 заменим базой
var listings = new[]
{
    new { id = 1, title = "2-комнатная, Юнусабад",  price = 62000, area = 54 },
    new { id = 2, title = "Студия, Чиланзар",       price = 41000, area = 30 },
    new { id = 3, title = "3-комнатная, Мирзо-Улугбек", price = 95000, area = 78 },
};

app.MapGet("/api/listings", () => listings);
app.MapGet("/api/listings/{id:int}", (int id) =>
    listings.FirstOrDefault(l => l.id == id) is {} found
        ? Results.Ok(found)
        : Results.NotFound());

app.Run();
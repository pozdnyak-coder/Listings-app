using ListingsApi.Contracts;
using ListingsApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ListingsApi.Endpoints;

public static class ListingEndpoints
{
    public static void MapListingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/listings").WithTags("Listings");

        group.MapGet("/", async (ListingService service, int? districtId, decimal? maxPrice) =>
            TypedResults.Ok(await service.GetAllAsync(districtId, maxPrice)))
        .WithName("GetListings")
        .WithSummary("Список объявлений с фильтром по району (id) и максимальной цене");

        // ВАЖНО: /count объявлен до /{id:int}
        group.MapGet("/count", async (ListingService service) =>
            TypedResults.Ok(new CountResponse(await service.CountAsync())))
        .WithName("CountListings")
        .WithSummary("Количество объявлений");

        group.MapGet("/{id:int}", async Task<Results<Ok<ListingResponse>, NotFound>>
            (int id, ListingService service) =>
        {
            var dto = await service.GetAsync(id);
            if (dto is null) return TypedResults.NotFound();
            return TypedResults.Ok(dto);
        })
        .WithName("GetListing")
        .WithSummary("Одно объявление по id");

        group.MapPost("/", async Task<Results<Created<ListingResponse>, BadRequest<string>>>
            (CreateListingRequest request, ListingService service) =>
        {
            var error = Validate(request.Title, request.Price, request.Rooms, request.Address);
            if (error is not null) return TypedResults.BadRequest(error);

            var created = await service.CreateAsync(request);
            if (created is null) return TypedResults.BadRequest("Район с таким districtId не найден");

            return TypedResults.Created($"/api/listings/{created.Id}", created);
        })
        .AddEndpointFilter<AdminKeyFilter>()
        .WithName("CreateListing")
        .WithSummary("Создать объявление");

        group.MapPut("/{id:int}", async Task<Results<NoContent, NotFound, BadRequest<string>>>
            (int id, UpdateListingRequest request, ListingService service) =>
        {
            var error = Validate(request.Title, request.Price, request.Rooms, request.Address);
            if (error is not null) return TypedResults.BadRequest(error);

            var result = await service.UpdateAsync(id, request);
            if (result == UpdateResult.NotFound) return TypedResults.NotFound();
            if (result == UpdateResult.BadDistrict) return TypedResults.BadRequest("Район с таким districtId не найден");
            return TypedResults.NoContent();
        })
        .AddEndpointFilter<AdminKeyFilter>()
        .WithName("UpdateListing")
        .WithSummary("Заменить объявление целиком");

        group.MapDelete("/{id:int}", async Task<Results<NoContent, NotFound>>
            (int id, ListingService service) =>
        {
            if (!await service.DeleteAsync(id)) return TypedResults.NotFound();
            return TypedResults.NoContent();
        })
        .AddEndpointFilter<AdminKeyFilter>()
        .WithName("DeleteListing")
        .WithSummary("Удалить объявление");
    }

    private static string? Validate(string? title, decimal price, int rooms, string? address)
    {
        if (string.IsNullOrWhiteSpace(title)) return "Поле title обязательно";
        if (title.Length > 200) return "Поле title — не длиннее 200 символов";
        if (price <= 0) return "Поле price должно быть больше 0";
        if (rooms is < 1 or > 10) return "Поле rooms — от 1 до 10";
        if (address is { Length: > 300 }) return "Поле address — не длиннее 300 символов";
        return null;
    }
}

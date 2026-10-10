using ListingsApi.Services;

namespace ListingsApi.Endpoints;

public static class DistrictEndpoints
{
    public static void MapDistrictEndpoints(this WebApplication app)
    {
        app.MapGet("/api/districts", async (DistrictService service) =>
                TypedResults.Ok(await service.GetAllAsync()))
           .WithTags("Districts")
           .WithName("GetDistricts")
           .WithSummary("Справочник районов с числом объявлений");
    }
}

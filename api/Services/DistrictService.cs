using ListingsApi.Contracts;
using ListingsApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Services;

public class DistrictService(AppDbContext db)
{
    public async Task<List<DistrictResponse>> GetAllAsync() =>
        await db.Districts.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DistrictResponse(d.Id, d.Name, d.Listings.Count))
            .ToListAsync();
}

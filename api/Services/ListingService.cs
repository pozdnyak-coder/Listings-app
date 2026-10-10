using System.Linq.Expressions;
using ListingsApi.Contracts;
using ListingsApi.Data;
using ListingsApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Services;

public enum UpdateResult { Ok, NotFound, BadDistrict }

public class ListingService(AppDbContext db)
{
    // Проекция в DTO — одна на все методы чтения (EF превращает её в SELECT с JOIN)
    private static readonly Expression<Func<Listing, ListingResponse>> ToResponse =
        l => new ListingResponse(l.Id, l.Title, l.Price, l.Rooms,
                                 l.DistrictId, l.District!.Name, l.Address, l.CreatedAt);

    public async Task<List<ListingResponse>> GetAllAsync(int? districtId, decimal? maxPrice)
    {
        var query = db.Listings.AsNoTracking().AsQueryable();

        if (districtId is not null) query = query.Where(l => l.DistrictId == districtId);
        if (maxPrice is not null)   query = query.Where(l => l.Price <= maxPrice);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(ToResponse)
            .ToListAsync();
    }

    public async Task<int> CountAsync() => await db.Listings.CountAsync();

    public async Task<ListingResponse?> GetAsync(int id) =>
        await db.Listings.AsNoTracking()
            .Where(l => l.Id == id)
            .Select(ToResponse)
            .FirstOrDefaultAsync();

    // null = район не найден (endpoint вернёт 400)
    public async Task<ListingResponse?> CreateAsync(CreateListingRequest r)
    {
        if (!await db.Districts.AnyAsync(d => d.Id == r.DistrictId))
            return null;

        var listing = new Listing
        {
            Title = r.Title.Trim(), Price = r.Price, Rooms = r.Rooms,
            Address = string.IsNullOrWhiteSpace(r.Address) ? null : r.Address.Trim(),
            DistrictId = r.DistrictId, CreatedAt = DateTime.UtcNow
        };
        db.Listings.Add(listing);
        await db.SaveChangesAsync();

        return (await GetAsync(listing.Id))!;
    }

    public async Task<UpdateResult> UpdateAsync(int id, UpdateListingRequest r)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return UpdateResult.NotFound;
        if (!await db.Districts.AnyAsync(d => d.Id == r.DistrictId)) return UpdateResult.BadDistrict;

        listing.Title = r.Title.Trim();
        listing.Price = r.Price;
        listing.Rooms = r.Rooms;
        listing.Address = string.IsNullOrWhiteSpace(r.Address) ? null : r.Address.Trim();
        listing.DistrictId = r.DistrictId;
        await db.SaveChangesAsync();
        return UpdateResult.Ok;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var listing = await db.Listings.FindAsync(id);
        if (listing is null) return false;

        db.Listings.Remove(listing);
        await db.SaveChangesAsync();
        return true;
    }
}

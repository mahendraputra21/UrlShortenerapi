using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly UrlDbContext _db;

    public UrlRepository(UrlDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(UrlMapping urlMapping)
    {
        await _db.UrlMappings.AddAsync(urlMapping);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(UrlMapping urlMapping)
    {
        _db.UrlMappings.Remove(urlMapping);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<UrlMapping>> GetAllAsync(int skip = 0, int take = 100)
    {
        return await _db.UrlMappings
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
    }

    public async Task<UrlMapping?> GetByIdAsync(Guid id)
    {
        return await _db.UrlMappings.FindAsync(id);
    }

    public async Task<UrlMapping?> GetByShortCodeAsync(string shortCode)
    {
        return await _db.UrlMappings
                .FirstOrDefaultAsync(x => x.ShortCode == shortCode);
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        return await _db.UrlMappings.AnyAsync(x => x.ShortCode == shortCode);
    }

    public async Task UpdateAsync(UrlMapping urlMapping)
    {
        _db.UrlMappings.Update(urlMapping);
        await _db.SaveChangesAsync();
    }
}

using Domain.Entities;

namespace Application.Interfaces;

public interface IUrlRepository
{
    Task<UrlMapping?> GetByShortCodeAsync(string shortCode);
    Task<UrlMapping?> GetByIdAsync(Guid id);
    Task<IEnumerable<UrlMapping>> GetAllAsync(int skip = 0, int take = 100);
    Task AddAsync(UrlMapping urlMapping);
    Task UpdateAsync(UrlMapping urlMapping);
    Task DeleteAsync(UrlMapping urlMapping);
    Task<bool> ShortCodeExistsAsync(string shortCode);
}

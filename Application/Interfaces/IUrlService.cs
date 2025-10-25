using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces;

public interface IUrlService
{
    Task<UrlDto> CreateShortUrlAsync(CreateUrlDto dto, string? ownerIp = null);
    Task<UrlMapping?> GetByShortCodeAsync(string shortCode);
    Task<IEnumerable<UrlDto>> GetAllAsync(int skip = 0, int take = 100);
    Task<UrlDto?> UpdateAsync(Guid id, UpdateUrlDto dto);
    Task<bool> DeleteAsync(Guid id);
}

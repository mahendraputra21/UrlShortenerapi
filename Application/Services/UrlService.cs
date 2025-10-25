using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _repo;
    private readonly ILogger<UrlService> _logger;
    private readonly Random _rng = new();

    public UrlService(IUrlRepository repo, ILogger<UrlService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<UrlDto> CreateShortUrlAsync(CreateUrlDto dto, string? ownerIp = null)
    {
        // validate URL
        if(!IsValidUrl(dto.LongUrl))
            throw new ArgumentException("The provided URL is not valid.", nameof(dto.LongUrl));

        // If custom code provided -> ensure uniqueness
        string code;
        if(!string.IsNullOrWhiteSpace(dto.CustomShortCode))
        {
            code = dto.CustomShortCode!;
            if (await _repo.ShortCodeExistsAsync(code))
                throw new InvalidOperationException("Custom short code already exists.");

        }
        else
        {
            // generate code and ensure unique (retry a few times)
            code = await GenerateUniqueCodeAsync();
        }

        var mapping = new UrlMapping
        {
            Id = Guid.NewGuid(),
            ShortCode = code,
            LongUrl = dto.LongUrl,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = dto.ExpirationUtc,
            OwnerIp = ownerIp
        };

        await _repo.AddAsync(mapping);
        return ToDto(mapping);
    }

    public async Task<UrlMapping?> GetByShortCodeAsync(string shortCode)
    {
        var mapping = await _repo.GetByShortCodeAsync(shortCode);
        return mapping;
    }

    public async Task<IEnumerable<UrlDto>> GetAllAsync(int skip = 0, int take = 100)
    {
        var all = await _repo.GetAllAsync(skip, take);
        return all.Select(ToDto);
    }

    public async Task<UrlDto?> UpdateAsync(Guid id, UpdateUrlDto dto)
    {
        var mapping = await _repo.GetByIdAsync(id);
        if (mapping == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.LongUrl))
        {
            if (!IsValidUrl(dto.LongUrl)) throw new ArgumentException("Invalid URL");
            mapping.LongUrl = dto.LongUrl;
        }

        mapping.ExpiresAt = dto.ExpirationUtc ?? mapping.ExpiresAt;
        await _repo.UpdateAsync(mapping);
        return ToDto(mapping);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var mapping = await _repo.GetByIdAsync(id);
        if (mapping == null) return false;
        await _repo.DeleteAsync(mapping);
        return true;
    }

    #region Private Methods
    private bool IsValidUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
        {
            return uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps;
        }
        return false;
    }

    private async Task<string> GenerateUniqueCodeAsync(int length = 7)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(alphabet[_rng.Next(alphabet.Length)]);
            var candidate = sb.ToString();
            if (!await _repo.ShortCodeExistsAsync(candidate))
                return candidate;
        }

        // fallback stronger loop
        while (true)
        {
            var timeBased = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                              .Replace("=", "")
                              .Replace("+", "")
                              .Replace("/", "")
                              .Substring(0, Math.Min(length, 11));
            if (!await _repo.ShortCodeExistsAsync(timeBased)) return timeBased;
        }
    }

    private static UrlDto ToDto(UrlMapping m) =>
           new()
           {
               Id = m.Id,
               ShortCode = m.ShortCode,
               LongUrl = m.LongUrl,
               CreatedAt = m.CreatedAt,
               ExpiresAt = m.ExpiresAt,
               HitCount = m.HitCount
           };
    #endregion
}

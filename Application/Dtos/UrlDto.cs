namespace Application.Dtos;

public class UrlDto
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = null!;
    public string LongUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long HitCount { get; set; }
    public string? ShortUrl => $"/r/{ShortCode}";
}

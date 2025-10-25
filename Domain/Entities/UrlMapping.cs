namespace Domain.Entities;

public class UrlMapping
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = null!; // unique short code
    public string LongUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long HitCount { get; set; }
    public string? OwnerIp { get; set; } // optional: who created it
}

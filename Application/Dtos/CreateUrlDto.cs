namespace Application.Dtos;

public class CreateUrlDto
{
    public string LongUrl { get; set; } = null!;
    public string? CustomShortCode { get; set; }
    public DateTime? ExpirationUtc { get; set; }
}

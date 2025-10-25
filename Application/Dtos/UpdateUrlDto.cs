namespace Application.Dtos;

public class UpdateUrlDto
{
    public string? LongUrl { get; set; }
    public DateTime? ExpirationUtc { get; set; }
}

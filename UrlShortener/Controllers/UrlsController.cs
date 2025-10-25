using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlsController : ControllerBase
    {
        private readonly IUrlService _service;
        private readonly ILogger<UrlsController> _logger;

        public UrlsController(IUrlService service, ILogger<UrlsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Create a shortened URL.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUrlDto dto)
        {
            try
            {
                var ownerIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var created = await _service.CreateShortUrlAsync(dto, ownerIp);
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var fullShort = $"{baseUrl}/r/{created.ShortCode}";
                return CreatedAtAction(nameof(GetByShortCode), new { shortCode = created.ShortCode }, new { created, shortUrl = fullShort });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get list of URL mappings.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var list = await _service.GetAllAsync(skip, take);
            return Ok(list);
        }

        /// <summary>
        /// Update an existing mapping.
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUrlDto dto)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, dto);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a mapping.
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (await _service.DeleteAsync(id))
                return NoContent();
            return NotFound();
        }

        /// <summary>
        /// Resolve short code and redirect.
        /// </summary>
        [HttpGet("/r/{shortCode}")]
        public async Task<IActionResult> GetByShortCode(string shortCode)
        {
            var mapping = await _service.GetByShortCodeAsync(shortCode);
            if (mapping == null) return NotFound();

            if (mapping.ExpiresAt.HasValue && mapping.ExpiresAt.Value < DateTime.UtcNow)
                return StatusCode(410, new { error = "Short URL expired" });

            // increment hit count and update repository
            mapping.HitCount++;
            await _service.UpdateAsync(mapping.Id, new UpdateUrlDto { }); // update hitcount persisted in repo via service
            return Redirect(mapping.LongUrl);
        }

        /// <summary>
        /// Get a single mapping by GUID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var mapping = await _service.GetAllAsync(0, 100); // simple list retrieval
            var found = mapping.FirstOrDefault(x => x.Id == id);
            if (found == null) return NotFound();
            return Ok(found);
        }
    }
}

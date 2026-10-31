using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Contact;

namespace Portfolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    // ── Public ───────────────────────────────────────────────────────

    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        await _contactMessageService.SubmitAsync(request, cancellationToken);
        return Ok(new { status = "received" });
    }

    // ── Admin ────────────────────────────────────────────────────────

    [HttpGet("messages")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMessages(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _contactMessageService.GetMessagesAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPut("messages/{id:int}/read")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var ok = await _contactMessageService.MarkReadAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpDelete("messages/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ok = await _contactMessageService.DeleteAsync(id, cancellationToken);
        if (!ok) return NotFound();
        return NoContent();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Contact;

namespace Portfolio.Api.Controllers;

/// <summary>
/// Public contact form endpoint. Depends only on IContactMessageService
/// (Application layer) - never touches EF Core directly, keeping the
/// "no business/DB logic in controllers" boundary real, not just a
/// convention (spec principle #7).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        // [ApiController] automatically returns 400 with a ProblemDetails
        // body if DataAnnotations validation on the request fails - no
        // manual ModelState check needed here.
        await _contactMessageService.SubmitAsync(request, cancellationToken);

        // Always return success, even if the honeypot silently dropped
        // the message - telling a bot "rejected" just teaches it to
        // adapt. A real sender still gets confirmation either way since
        // their submission genuinely succeeded.
        return Ok(new { status = "received" });
    }
}

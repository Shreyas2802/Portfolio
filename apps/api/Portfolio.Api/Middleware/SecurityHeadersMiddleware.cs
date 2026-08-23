namespace Portfolio.Api.Middleware;

/// <summary>
/// Adds security headers to every response. Built around what this
/// API actually does, not copied from a generic template: it serves
/// JSON (never HTML with embedded scripts) and static image variants
/// from /uploads - no inline scripts, no third-party CDNs, no
/// external fonts (self-hosted, see Phase 1), so the CSP here can be
/// tight without carve-outs.
///
/// Important caveat: in local dev, the actual page the browser loads
/// (the React SPA's HTML) is served by Vite's dev server on a
/// different port, not by this API - these headers protect this
/// API's own responses (JSON, /uploads images) but don't cover the
/// SPA's document response until something serves that document from
/// this same pipeline. Phase 10 (Deployment) needs to ensure whatever
/// serves the built SPA in production applies equivalent headers -
/// either by having this API serve the built SPA too, or by
/// replicating this middleware on that host.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _isDevelopment;

    public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _isDevelopment = env.IsDevelopment();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
        headers["X-Frame-Options"] = "DENY";
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "img-src 'self' data:; " +
            "script-src 'self'; " +
            "style-src 'self'; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        // HSTS only makes sense over HTTPS, and only outside Development -
        // caching it in a browser during local HTTP dev work causes real
        // annoyance (forced HTTPS redirects that don't match the dev setup).
        if (!_isDevelopment)
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await _next(context);
    }
}

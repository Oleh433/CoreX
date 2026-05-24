namespace CoreX.Http;

public static class HttpRequestExtensions
{
    // HTMX sets HX-Request: true on every request it issues (including hx-boost
    // navigations). Use this to branch between "full page" and "partial swap" responses.
    public static bool IsHtmx(this HttpRequest request) =>
        request.Headers.TryGetValue("HX-Request", out var v) && v == "true";
}

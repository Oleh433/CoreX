using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace CoreX.UI.Tests.TestSupport;

// Helpers that round-trip Razor Pages antiforgery: GET a page, scrape the
// __RequestVerificationToken hidden input + cookie, then POST the form with
// both the body field and the cookie attached.
public static class AntiforgeryClient
{
    private static readonly Regex TokenRegex =
        new(@"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
            RegexOptions.Compiled);

    public static async Task<(string Token, string Cookie)> FetchAsync(HttpClient client, string url)
    {
        var get = await client.GetAsync(url);
        var html = await get.Content.ReadAsStringAsync();
        var match = TokenRegex.Match(html);
        if (!match.Success)
            throw new InvalidOperationException($"No antiforgery token found at {url}.");

        var cookies = get.Headers.TryGetValues("Set-Cookie", out var values) ? values : Array.Empty<string>();
        var afCookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Antiforgery", StringComparison.Ordinal));
        // If the server didn't reissue the antiforgery cookie (because the client already
        // has one in its handler's cookie jar from a previous request — e.g. after sign-in),
        // return an empty cookie string. BuildPost relies on the handler's automatic cookie
        // attachment in that case.
        var cookieValue = afCookie is null ? string.Empty : afCookie.Split(';')[0];
        return (match.Groups["token"].Value, cookieValue);
    }

    public static HttpRequestMessage BuildPost(
        string url,
        IEnumerable<KeyValuePair<string, string>> form,
        string antiforgeryToken,
        string antiforgeryCookie,
        string? extraCookie = null)
    {
        var fields = form.Append(new("__RequestVerificationToken", antiforgeryToken));
        var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(fields),
        };
        // If antiforgeryCookie is empty, the cookie is already in the client handler's jar
        // (e.g. carried over from a previous request after sign-in) and will be attached
        // automatically — skip the explicit Cookie header to avoid duplication conflicts.
        var hasAfCookie = !string.IsNullOrEmpty(antiforgeryCookie);
        if (hasAfCookie || extraCookie is not null)
        {
            var cookieHeader = hasAfCookie
                ? (extraCookie is null ? antiforgeryCookie : $"{antiforgeryCookie}; {extraCookie}")
                : extraCookie!;
            req.Headers.Add("Cookie", cookieHeader);
        }
        return req;
    }
}

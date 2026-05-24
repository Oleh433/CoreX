namespace CoreX.UI.Tests.TestSupport;

public static class HtmxClient
{
    public static Task<HttpResponseMessage> GetHxAsync(this HttpClient client, string url)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("HX-Request", "true");
        return client.SendAsync(req);
    }
}

using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CoreX.UI.Tests.Pages;

public class IndexTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;

    public IndexTests(CoreXFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Index_ReturnsOk_AndUkrainianHeadline()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Перетни свою межу.", body);
        Assert.Contains("Знайти клуб", body);
        Assert.DoesNotContain("Push your limit.", body);
    }

    [Fact]
    public async Task Get_Index_WithEnglishCulture_ReturnsEnglishHeadline()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Cookie", $"{Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName}=c=en|uic=en");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Push your limit.", body);
        Assert.Contains("Find a club", body);
        Assert.DoesNotContain("Перетни свою межу.", body);
    }
}

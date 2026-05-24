using System.Net;
using CoreX.UI.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoreX.UI.Tests.Pages.Trainers;

public class DetailTests : IClassFixture<CoreXFactory>
{
    private readonly CoreXFactory _factory;
    public DetailTests(CoreXFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_TrainerDetail_ShowsTrainerInfo()
    {
        var clubs = await SeedData.SeedDiscoveryFixtureAsync(_factory);
        // Find the trainer for clubA via the service (Phase 2 tests can use DI scope).
        using var scope = _factory.Services.CreateScope();
        var trainerService = scope.ServiceProvider.GetRequiredService<CoreX.Application.ServiceInterfaces.ITrainerService>();
        var trainers = await trainerService.GetByClubIdAsync(clubs[0].Id);
        var trainerId = trainers[0].Id;

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Trainers/{trainerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ірина Швець", body);
        Assert.Contains("Силові", body);
    }

    [Fact]
    public async Task Get_TrainerDetail_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/Trainers/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

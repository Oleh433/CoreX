using CoreX.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CoreX.UI.Tests.TestSupport;

public static class TestUsers
{
    public const string DefaultPassword = "TestUserPass1!";

    // Creates an ApplicationUser via UserManager in the factory's service scope and
    // assigns the given role (creating the role if missing — the IdentityInitializer
    // runs at startup so default roles already exist).
    public static async Task<ApplicationUser> CreateAsync(
        CoreXFactory factory,
        string email,
        string role,
        string fullName = "Test User",
        string password = DefaultPassword)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FullName = fullName,
            EmailConfirmed = true,
        };

        var createResult = await users.CreateAsync(user, password);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"CreateAsync failed: {string.Join("; ", createResult.Errors.Select(e => e.Description))}");

        var roleResult = await users.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException(
                $"AddToRoleAsync failed: {string.Join("; ", roleResult.Errors.Select(e => e.Description))}");

        return user;
    }

    // Returns an HttpClient that has signed in via /Account/Login. The
    // factory's HttpClient handler keeps cookies between requests when
    // HandleCookies = true (the WebApplicationFactory default).
    public static async Task<HttpClient> SignedInClientAsync(
        CoreXFactory factory,
        string email,
        string password = DefaultPassword)
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var (token, afCookie) = await AntiforgeryClient.FetchAsync(client, "/Account/Login");
        var post = AntiforgeryClient.BuildPost(
            "/Account/Login",
            new Dictionary<string, string>
            {
                ["Input.Email"] = email,
                ["Input.Password"] = password,
            },
            token,
            afCookie);

        var response = await client.SendAsync(post);
        if (response.StatusCode != System.Net.HttpStatusCode.Redirect &&
            response.StatusCode != System.Net.HttpStatusCode.Found)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Sign-in did not redirect (status {(int)response.StatusCode}). Body:\n{body}");
        }

        // CreateClient gives an HttpClient with a handler that captures cookies between
        // requests; the auth cookie set by the POST is now part of its store.
        return client;
    }

    public static async Task<HttpClient> SignedInAsAdminAsync(CoreXFactory factory, string fullName = "Test Admin")
    {
        var email = $"admin-{Guid.NewGuid():N}@test";
        await CreateAsync(factory, email, role: "Admin", fullName: fullName);
        return await SignedInClientAsync(factory, email);
    }

    public static async Task<HttpClient> SignedInAsOwnerAsync(CoreXFactory factory, string fullName = "Test Owner")
    {
        var email = $"owner-{Guid.NewGuid():N}@test";
        await CreateAsync(factory, email, role: "Owner", fullName: fullName);
        return await SignedInClientAsync(factory, email);
    }
}

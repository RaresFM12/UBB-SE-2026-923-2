using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

/// <summary>
/// Happy-path controller integration tests for the ER Dispatch web slice:
/// the assignment requires at least <c>Index</c> and <c>Create [POST]</c>.
/// </summary>
[TestFixture]
public class ErDispatchControllerIntegrationTests
{
    private ErDispatchWebApplicationFactory factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void Setup()
    {
        factory = new ErDispatchWebApplicationFactory();
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task Index_AuthenticatedAdmin_ReturnsDashboard()
    {
        var response = await client.GetAsync("/ERDispatch");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("ER Dispatch"));
    }

    [Test]
    public async Task Create_Post_ValidRequest_RedirectsAndPersists()
    {
        var token = await GetAntiForgeryTokenAsync("/ERDispatch/Create");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Specialization"] = "Cardiology",
            ["Location"] = "Ward A",
            ["__RequestVerificationToken"] = token,
        });

        var postResponse = await client.PostAsync("/ERDispatch/Create", form);

        Assert.That(
            postResponse.StatusCode,
            Is.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found));

        var indexResponse = await client.GetAsync("/ERDispatch");
        var indexHtml = await indexResponse.Content.ReadAsStringAsync();
        Assert.That(indexHtml, Does.Contain("Cardiology"));
    }

    [Test]
    public async Task Edit_Post_EngineOwnedStatus_IsRejectedAndNotPersisted()
    {
        // Arrange: create a PENDING request through the UI.
        var createToken = await GetAntiForgeryTokenAsync("/ERDispatch/Create");
        await client.PostAsync("/ERDispatch/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Specialization"] = "EditGuardTest",
            ["Location"] = "Ward A",
            ["__RequestVerificationToken"] = createToken,
        }));

        var indexHtml = await (await client.GetAsync("/ERDispatch")).Content.ReadAsStringAsync();
        var id = Regex.Match(indexHtml, @"/ERDispatch/Edit/(\d+)").Groups[1].Value;
        Assert.That(id, Is.Not.Empty, "Created request id not found on dashboard");

        // Act: tamper the form to an engine-owned status the desktop can never set.
        var editToken = await GetAntiForgeryTokenAsync($"/ERDispatch/Edit/{id}");
        var tampered = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = id,
            ["Specialization"] = "EditGuardTest",
            ["Location"] = "Ward A",
            ["Status"] = "ASSIGNED",
            ["__RequestVerificationToken"] = editToken,
        });
        var response = await client.PostAsync("/ERDispatch/Edit", tampered);

        // Assert: rejected (re-rendered 200, not a 302 redirect, not a 500),
        // and the request is still PENDING.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var detailsHtml = await (await client.GetAsync($"/ERDispatch/Details/{id}")).Content.ReadAsStringAsync();
        Assert.That(detailsHtml, Does.Contain(">PENDING<"));
        Assert.That(detailsHtml, Does.Not.Contain(">ASSIGNED<"));
    }

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var pageResponse = await client.GetAsync(url);
        Assert.That(pageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var html = await pageResponse.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

        Assert.That(match.Success, Is.True, $"Anti-forgery token not found on {url}");
        return match.Groups[1].Value;
    }
}

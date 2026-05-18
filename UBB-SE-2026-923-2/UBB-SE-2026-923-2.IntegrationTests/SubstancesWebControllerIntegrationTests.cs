using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class SubstancesWebControllerIntegrationTests
{
    private const string SubstanceName = "Ibuprofen";
    private const string SubstanceLethalDose = "600";
    private const string SubstanceDescription = "Non-steroidal anti-inflammatory drug";

    private AdminWebApplicationFactory factory;
    private HttpClient httpClient;

    [SetUp]
    public void Setup()
    {
        this.factory = new AdminWebApplicationFactory();
        this.httpClient = this.factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [TearDown]
    public void TearDown()
    {
        this.httpClient.Dispose();
        this.factory.Dispose();
    }

    [Test]
    public async Task Index_WhenCalled_ReturnsOk()
    {
        HttpResponseMessage response = await this.httpClient.GetAsync("/Substances");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_WhenCalledWithValidSubstance_RedirectsToIndex()
    {
        HttpResponseMessage getResponse = await this.httpClient.GetAsync("/Substances/Create");
        string html = await getResponse.Content.ReadAsStringAsync();
        string token = AdminWebApplicationFactory.ExtractAntiForgeryToken(html);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Name"] = SubstanceName,
            ["LethalDose"] = SubstanceLethalDose,
            ["Description"] = SubstanceDescription,
        };

        HttpResponseMessage postResponse = await this.httpClient.PostAsync("/Substances/Create", new FormUrlEncodedContent(formData));

        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(postResponse.Headers.Location?.ToString(), Does.Contain("/Substances"));
    }
}

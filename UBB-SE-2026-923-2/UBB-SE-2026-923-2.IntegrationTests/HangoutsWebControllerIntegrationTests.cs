using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class HangoutsWebControllerIntegrationTests
{
    private const string HangoutTitle = "Team Coffee Break";
    private const string HangoutDescription = "Quick coffee meetup";
    private const string HangoutDateFormat = "yyyy-MM-ddTHH:mm";
    private const int HangoutMinimumDaysInAdvance = 8;
    private const string HangoutMaxParticipants = "5";
    private const string HangoutSelectedDoctorId = "1";

    private HangoutsWebApplicationFactory factory;
    private HttpClient httpClient;

    [SetUp]
    public void Setup()
    {
        this.factory = new HangoutsWebApplicationFactory();
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
        HttpResponseMessage response = await this.httpClient.GetAsync("/Hangouts");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_WhenCalledWithValidHangout_RedirectsToIndex()
    {
        HttpResponseMessage getResponse = await this.httpClient.GetAsync("/Hangouts/Create");
        string html = await getResponse.Content.ReadAsStringAsync();
        string token = HangoutsWebApplicationFactory.ExtractAntiForgeryToken(html);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Title"] = HangoutTitle,
            ["Description"] = HangoutDescription,
            ["Date"] = DateTime.Now.AddDays(HangoutMinimumDaysInAdvance).ToString(HangoutDateFormat),
            ["MaxParticipantsCount"] = HangoutMaxParticipants,
            ["SelectedDoctorId"] = HangoutSelectedDoctorId,
        };

        HttpResponseMessage postResponse = await this.httpClient.PostAsync("/Hangouts/Create", new FormUrlEncodedContent(formData));

        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(postResponse.Headers.Location?.ToString(), Does.Contain("/Hangouts"));
    }
}

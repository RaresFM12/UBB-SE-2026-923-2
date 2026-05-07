using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class NotificationsControllerAdditionalTests
{
    private CustomWebApplicationFactory factory;
    private HttpClient client;

    [SetUp]
    public void Setup()
    {
        factory = new CustomWebApplicationFactory();
        client = factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task CreateNotification_EmptyMessage_ReturnsNoContent()
    {
        var request = new { RecipientStaffId = 1, Title = "T", Message = "" };
        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateNotification_LongTitle_ReturnsNoContent()
    {
        var request = new { RecipientStaffId = 1, Title = new string('T', 300), Message = "M" };
        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateNotification_ZeroRecipientId_ReturnsNoContent()
    {
        var request = new { RecipientStaffId = 0, Title = "T", Message = "M" };
        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateNotification_HighRecipientId_ReturnsNoContent()
    {
        var request = new { RecipientStaffId = 99999, Title = "T", Message = "M" };
        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateNotification_SpecialCharsInTitle_ReturnsNoContent()
    {
        var request = new { RecipientStaffId = 1, Title = "Alert! @#$ <b>Bold</b>", Message = "M" };
        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}

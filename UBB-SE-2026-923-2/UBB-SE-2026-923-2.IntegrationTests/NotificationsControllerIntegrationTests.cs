using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class NotificationsControllerIntegrationTests
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
    public async Task CreateNotification_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            RecipientStaffId = 1,
            Title = "Shift Reminder",
            Message = "Your shift starts in 1 hour"
        };

        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateNotification_EmptyTitle_DoesNotReturnServerError()
    {
        var request = new
        {
            RecipientStaffId = 1,
            Title = "",
            Message = "Test message"
        };

        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateNotification_LongMessage_ReturnsNoContent()
    {
        var request = new
        {
            RecipientStaffId = 2,
            Title = "Alert",
            Message = new string('X', 500)
        };

        var response = await client.PostAsJsonAsync("/api/Notifications", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateMultipleNotifications_AllReturnNoContent()
    {
        for (int i = 0; i < 3; i++)
        {
            var request = new { RecipientStaffId = i + 1, Title = $"Title{i}", Message = $"Msg{i}" };
            var response = await client.PostAsJsonAsync("/api/Notifications", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }
    }
}

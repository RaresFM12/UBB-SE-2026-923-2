using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class OrdersControllerIntegrationTests
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
    public async Task GetAllOrders_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Orders");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
        Assert.That(orders!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateOrder_ValidRequest_ReturnsOrderId()
    {
        var request = new
        {
            ClientId = 1,
            PickUpDate = "2025-06-01",
            IsCompleted = false,
            IsExpired = false
        };

        var response = await client.PostAsJsonAsync("/api/Orders", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetOrderById_NonExistentId_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Orders/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetOrderExists_NonExistentId_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Orders/9999/exists");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetOrdersByClientId_NoOrders_ReturnsEmptyList()
    {
        var orders = await client.GetFromJsonAsync<List<Order>>("/api/Orders?clientId=999");
        Assert.That(orders!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteOrder_NonExistentId_DoesNotReturnServerError()
    {
        var response = await client.DeleteAsync("/api/Orders/9999");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateOrder_ThenGetById_ReturnsOk()
    {
        var request = new
        {
            ClientId = 1,
            PickUpDate = "2025-07-01",
            IsCompleted = false,
            IsExpired = false
        };
        var createResponse = await client.PostAsJsonAsync("/api/Orders", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var response = await client.GetAsync($"/api/Orders/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateOrder_ThenExists_ReturnsTrue()
    {
        var request = new
        {
            ClientId = 2,
            PickUpDate = "2025-08-01",
            IsCompleted = false,
            IsExpired = false
        };
        var createResponse = await client.PostAsJsonAsync("/api/Orders", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var result = await client.GetFromJsonAsync<bool>($"/api/Orders/{id}/exists");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CreateOrder_ThenDelete_DoesNotReturnServerError()
    {
        var request = new
        {
            ClientId = 3,
            PickUpDate = "2025-09-01",
            IsCompleted = false,
            IsExpired = false
        };
        var createResponse = await client.PostAsJsonAsync("/api/Orders", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var response = await client.DeleteAsync($"/api/Orders/{id}");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task CreateOrder_ThenGetByClientId_ReturnsNonEmpty()
    {
        var request = new
        {
            ClientId = 77,
            PickUpDate = "2025-10-01",
            IsCompleted = false,
            IsExpired = false
        };
        await client.PostAsJsonAsync("/api/Orders", request);
        var orders = await client.GetFromJsonAsync<List<Order>>("/api/Orders?clientId=77");
        Assert.That(orders!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task CreateOrder_CompletedFlag_Persists()
    {
        var request = new
        {
            ClientId = 4,
            PickUpDate = "2025-11-01",
            IsCompleted = true,
            IsExpired = false
        };
        var createResponse = await client.PostAsJsonAsync("/api/Orders", request);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();
        var order = await client.GetFromJsonAsync<Order>($"/api/Orders/{id}");
        Assert.That(order!.IsCompleted, Is.True);
    }
}

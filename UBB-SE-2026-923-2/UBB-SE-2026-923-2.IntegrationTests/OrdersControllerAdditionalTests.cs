using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class OrdersControllerAdditionalTests
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
    public async Task CreateOrder_CompletedAndExpired_Persists()
    {
        var request = new { ClientId = 1, PickUpDate = "2025-12-01", IsCompleted = true, IsExpired = true };
        var response = await client.PostAsJsonAsync("/api/Orders", request);
        var id = await response.Content.ReadFromJsonAsync<int>();
        var order = await client.GetFromJsonAsync<Order>($"/api/Orders/{id}");
        Assert.That(order!.IsExpired, Is.True);
    }

    [Test]
    public async Task CreateMultipleOrders_GetAll_ReturnsCorrectCount()
    {
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 1, PickUpDate = "2025-06-01", IsCompleted = false, IsExpired = false });
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 2, PickUpDate = "2025-06-02", IsCompleted = false, IsExpired = false });
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 3, PickUpDate = "2025-06-03", IsCompleted = false, IsExpired = false });
        var orders = await client.GetFromJsonAsync<List<Order>>("/api/Orders");
        Assert.That(orders!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task CreateOrder_ThenUpdate_ChangesPickUpDate()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 5, PickUpDate = "2025-07-01", IsCompleted = false, IsExpired = false });
        var id = await response.Content.ReadFromJsonAsync<int>();
        var order = await client.GetFromJsonAsync<Order>($"/api/Orders/{id}");
        order!.IsCompleted = true;
        var updateResponse = await client.PutAsJsonAsync($"/api/Orders/{id}", order);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateOrder_ThenUpdate_ThenGet_ReflectsUpdate()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 6, PickUpDate = "2025-08-01", IsCompleted = false, IsExpired = false });
        var id = await response.Content.ReadFromJsonAsync<int>();
        var order = await client.GetFromJsonAsync<Order>($"/api/Orders/{id}");
        order!.IsCompleted = true;
        await client.PutAsJsonAsync($"/api/Orders/{id}", order);
        var updated = await client.GetFromJsonAsync<Order>($"/api/Orders/{id}");
        Assert.That(updated!.IsCompleted, Is.True);
    }

    [Test]
    public async Task CreateOrder_DifferentClients_FilterByClientReturnsOne()
    {
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 100, PickUpDate = "2025-06-01", IsCompleted = false, IsExpired = false });
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 200, PickUpDate = "2025-06-02", IsCompleted = false, IsExpired = false });
        var orders = await client.GetFromJsonAsync<List<Order>>("/api/Orders?clientId=100");
        Assert.That(orders!.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateOrder_ThenDelete_ThenGetById_ReturnsNotFound()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 7, PickUpDate = "2025-09-01", IsCompleted = false, IsExpired = false });
        var id = await response.Content.ReadFromJsonAsync<int>();
        await client.DeleteAsync($"/api/Orders/{id}");
        var getResponse = await client.GetAsync($"/api/Orders/{id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateOrder_ThenDelete_ThenExists_ReturnsFalse()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 8, PickUpDate = "2025-10-01", IsCompleted = false, IsExpired = false });
        var id = await response.Content.ReadFromJsonAsync<int>();
        await client.DeleteAsync($"/api/Orders/{id}");
        var exists = await client.GetFromJsonAsync<bool>($"/api/Orders/{id}/exists");
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetAllOrders_MultipleClients_ReturnsAll()
    {
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 300, PickUpDate = "2025-06-01", IsCompleted = false, IsExpired = false });
        await client.PostAsJsonAsync("/api/Orders", new { ClientId = 300, PickUpDate = "2025-06-02", IsCompleted = true, IsExpired = false });
        var orders = await client.GetFromJsonAsync<List<Order>>("/api/Orders");
        Assert.That(orders!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task CreateOrder_FuturePickUpDate_ReturnsOk()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 9, PickUpDate = "2026-01-01", IsCompleted = false, IsExpired = false });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateOrder_PastPickUpDate_ReturnsOk()
    {
        var response = await client.PostAsJsonAsync("/api/Orders", new { ClientId = 10, PickUpDate = "2020-01-01", IsCompleted = false, IsExpired = false });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

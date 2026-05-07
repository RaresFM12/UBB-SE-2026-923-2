using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ItemsControllerIntegrationTests
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
    public async Task GetAllItems_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Items");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var items = await response.Content.ReadFromJsonAsync<List<Item>>();
        Assert.That(items, Is.Not.Null);
        Assert.That(items!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateItem_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            Name = "Aspirin",
            Producer = "Bayer",
            Category = "pain",
            Price = 10.0f,
            NumberOfPills = 30,
            Label = "OTC",
            Description = "Pain reliever",
            ImagePath = "/img/aspirin.png",
            Discount = 0.0f
        };

        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItemAndGetAll_ReturnsCreatedItem()
    {
        var request = new
        {
            Name = "Ibuprofen",
            Producer = "Advil",
            Category = "pain",
            Price = 15.0f,
            NumberOfPills = 20,
            Label = "OTC",
            Description = "Anti-inflammatory",
            ImagePath = "/img/ibu.png",
            Discount = 0.0f
        };

        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        Assert.That(items!.Any(i => i.Name == "Ibuprofen"), Is.True);
    }

    [Test]
    public async Task GetItemById_NonExistentId_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Items/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetItemExists_NonExistentId_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Items/9999/exists");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetItemsByName_NoMatch_ReturnsEmptyList()
    {
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items?name=NonExistent");
        Assert.That(items!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteItem_NonExistentId_DoesNotThrow()
    {
        var response = await client.DeleteAsync("/api/Items/9999");
        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
    }

    [Test]
    public async Task GetTopItems_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Items/top");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateItem_ThenGetById_ReturnsOk()
    {
        var request = new
        {
            Name = "TestDrug",
            Producer = "TestProducer",
            Category = "cat",
            Price = 5.0f,
            NumberOfPills = 10,
            Label = "OTC",
            Description = "desc",
            ImagePath = "/img/test.png",
            Discount = 0.0f
        };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var id = items!.First().Id;
        var response = await client.GetAsync($"/api/Items/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateItem_ThenExists_ReturnsTrue()
    {
        var request = new
        {
            Name = "ExistsDrug",
            Producer = "P",
            Category = "cat",
            Price = 3.0f,
            NumberOfPills = 5,
            Label = "Rx",
            Description = "d",
            ImagePath = "/img/e.png",
            Discount = 0.0f
        };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var id = items!.First().Id;
        var result = await client.GetFromJsonAsync<bool>($"/api/Items/{id}/exists");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CreateItem_ThenGetByName_ReturnsMatch()
    {
        var request = new
        {
            Name = "UniqueNameDrug",
            Producer = "P",
            Category = "cat",
            Price = 7.0f,
            NumberOfPills = 14,
            Label = "OTC",
            Description = "d",
            ImagePath = "/img/u.png",
            Discount = 0.0f
        };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items?name=UniqueNameDrug");
        Assert.That(items!.Count, Is.GreaterThan(0));
    }

    [Test]
    public async Task CreateItem_ThenDelete_ReturnsNoContent()
    {
        var request = new
        {
            Name = "DeleteMe",
            Producer = "P",
            Category = "cat",
            Price = 1.0f,
            NumberOfPills = 1,
            Label = "OTC",
            Description = "d",
            ImagePath = "/img/d.png",
            Discount = 0.0f
        };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var id = items!.First().Id;
        var response = await client.DeleteAsync($"/api/Items/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItem_ThenUpdate_ReturnsNoContent()
    {
        var request = new
        {
            Name = "UpdateMe",
            Producer = "P",
            Category = "cat",
            Price = 2.0f,
            NumberOfPills = 2,
            Label = "OTC",
            Description = "d",
            ImagePath = "/img/up.png",
            Discount = 0.0f
        };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var item = items!.First();
        item.Price = 99.0f;
        var response = await client.PutAsJsonAsync($"/api/Items/{item.Id}", item);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}

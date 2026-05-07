using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ItemsControllerAdditionalTests
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
    public async Task CreateItemWithQuantity_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            Name = "DrugQ",
            Producer = "P",
            Category = "pain",
            Price = 12.0f,
            NumberOfPills = 20,
            Quantity = 100,
            ActiveSubstances = new Dictionary<string, float> { { "acid", 250f } },
            Batches = new Dictionary<string, int> { { DateOnly.FromDateTime(DateTime.Now.AddDays(60)).ToString("yyyy-MM-dd"), 100 } },
            Label = "Rx",
            Description = "desc",
            ImagePath = "/img/q.png",
            Discount = 0.1f
        };
        var response = await client.PostAsJsonAsync("/api/Items/with-quantity", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetAllItems_AfterCreate_ContainsItemName()
    {
        var request = new { Name = "VerifyName", Producer = "P", Category = "c", Price = 1f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        Assert.That(items!.Any(i => i.Name == "VerifyName"), Is.True);
    }

    [Test]
    public async Task CreateItem_ZeroPrice_ReturnsNoContent()
    {
        var request = new { Name = "Free", Producer = "P", Category = "c", Price = 0f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItem_HighPrice_ReturnsNoContent()
    {
        var request = new { Name = "Expensive", Producer = "P", Category = "c", Price = 99999f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItem_WithDiscount_ReturnsNoContent()
    {
        var request = new { Name = "Discounted", Producer = "P", Category = "c", Price = 50f, NumberOfPills = 10, Label = "L", Description = "D", ImagePath = "/i", Discount = 0.25f };
        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItem_LongName_ReturnsNoContent()
    {
        var request = new { Name = new string('A', 200), Producer = "P", Category = "c", Price = 1f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateItem_EmptyCategory_ReturnsNoContent()
    {
        var request = new { Name = "NoCat", Producer = "P", Category = "", Price = 1f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        var response = await client.PostAsJsonAsync("/api/Items", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task UpdateItem_AfterCreate_PriceChanges()
    {
        var request = new { Name = "ToUpdate", Producer = "P", Category = "c", Price = 10f, NumberOfPills = 5, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var item = items!.First();
        item.Price = 25f;
        await client.PutAsJsonAsync($"/api/Items/{item.Id}", item);
        var updated = await client.GetFromJsonAsync<Item>($"/api/Items/{item.Id}");
        Assert.That(updated!.Price, Is.EqualTo(25f));
    }

    [Test]
    public async Task DeleteItem_AfterCreate_ItemNoLongerExists()
    {
        var request = new { Name = "ToDelete", Producer = "P", Category = "c", Price = 1f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        await client.PostAsJsonAsync("/api/Items", request);
        var items = await client.GetFromJsonAsync<List<Item>>("/api/Items");
        var id = items!.First().Id;
        await client.DeleteAsync($"/api/Items/{id}");
        var exists = await client.GetFromJsonAsync<bool>($"/api/Items/{id}/exists");
        Assert.That(exists, Is.False);
    }

    [Test]
    public async Task GetItemsByName_PartialMatch_ReturnsResults()
    {
        var request = new { Name = "PartialTest", Producer = "P", Category = "c", Price = 1f, NumberOfPills = 1, Label = "L", Description = "D", ImagePath = "/i", Discount = 0f };
        await client.PostAsJsonAsync("/api/Items", request);
        var response = await client.GetAsync("/api/Items?name=PartialTest");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}

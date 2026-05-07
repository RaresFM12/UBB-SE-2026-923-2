using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class UsersControllerAdditionalTests
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
    public async Task CreateUser_AdminRole_ReturnsNoContent()
    {
        var request = new { Email = "admin@test.com", PhoneNumber = "07000", PasswordHash = "h", Username = "admin1", DiscountNotifications = false, IsDisabled = false, IsAdmin = true, LoyaltyPoints = 0, Role = "Admin" };
        var response = await client.PostAsJsonAsync("/api/Users", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateUser_DisabledAccount_ReturnsNoContent()
    {
        var request = new { Email = "disabled@test.com", PhoneNumber = "07001", PasswordHash = "h", Username = "dis1", DiscountNotifications = false, IsDisabled = true, IsAdmin = false, LoyaltyPoints = 0, Role = "Patient" };
        var response = await client.PostAsJsonAsync("/api/Users", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateUser_HighLoyaltyPoints_ReturnsNoContent()
    {
        var request = new { Email = "loyal@test.com", PhoneNumber = "07002", PasswordHash = "h", Username = "loyal1", DiscountNotifications = true, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 99999, Role = "Patient" };
        var response = await client.PostAsJsonAsync("/api/Users", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateUser_WithDiscountNotifications_Persists()
    {
        var request = new { Email = "notif@test.com", PhoneNumber = "07003", PasswordHash = "h", Username = "notif1", DiscountNotifications = true, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "Patient" };
        await client.PostAsJsonAsync("/api/Users", request);
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        Assert.That(users!.Any(u => u.Username == "notif1"), Is.True);
    }

    [Test]
    public async Task CreateMultipleUsers_GetAll_ReturnsCorrectCount()
    {
        await client.PostAsJsonAsync("/api/Users", new { Email = "a@t.com", PhoneNumber = "1", PasswordHash = "h", Username = "u1", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "P" });
        await client.PostAsJsonAsync("/api/Users", new { Email = "b@t.com", PhoneNumber = "2", PasswordHash = "h", Username = "u2", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "P" });
        await client.PostAsJsonAsync("/api/Users", new { Email = "c@t.com", PhoneNumber = "3", PasswordHash = "h", Username = "u3", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "P" });
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        Assert.That(users!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task UpdateUser_ChangeLoyaltyPoints_Persists()
    {
        await client.PostAsJsonAsync("/api/Users", new { Email = "lp@t.com", PhoneNumber = "4", PasswordHash = "h", Username = "lp1", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 10, Role = "P" });
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        var user = users!.First();
        user.LoyaltyPoints = 500;
        await client.PutAsJsonAsync($"/api/Users/{user.Id}", user);
        var updated = await client.GetFromJsonAsync<User>($"/api/Users/{user.Id}");
        Assert.That(updated!.LoyaltyPoints, Is.EqualTo(500));
    }

    [Test]
    public async Task UpdateUser_ChangeUsername_Persists()
    {
        await client.PostAsJsonAsync("/api/Users", new { Email = "un@t.com", PhoneNumber = "5", PasswordHash = "h", Username = "oldname", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "P" });
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        var user = users!.First();
        user.Username = "newname";
        await client.PutAsJsonAsync($"/api/Users/{user.Id}", user);
        var updated = await client.GetFromJsonAsync<User>($"/api/Users/{user.Id}");
        Assert.That(updated!.Username, Is.EqualTo("newname"));
    }

    [Test]
    public async Task GetUserByEmail_EmptyString_ReturnsBadRequest()
    {
        var response = await client.GetAsync("/api/Users/by-email?email=");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetUserExistsByEmail_NonExistent_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Users/exists?email=ghost@nowhere.com");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CreateUser_EmptyPassword_ReturnsNoContent()
    {
        var request = new { Email = "ep@t.com", PhoneNumber = "6", PasswordHash = "", Username = "ep1", DiscountNotifications = false, IsDisabled = false, IsAdmin = false, LoyaltyPoints = 0, Role = "P" };
        var response = await client.PostAsJsonAsync("/api/Users", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}

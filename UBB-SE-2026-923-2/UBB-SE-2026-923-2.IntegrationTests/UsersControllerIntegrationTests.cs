using System.Net;
using System.Net.Http.Json;
using NUnit.Framework;
using UBB_SE_2026_923_2.Models;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class UsersControllerIntegrationTests
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
    public async Task GetAllUsers_EmptyDatabase_ReturnsEmptyList()
    {
        var response = await client.GetAsync("/api/Users");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var users = await response.Content.ReadFromJsonAsync<List<User>>();
        Assert.That(users!.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task CreateUser_ValidRequest_ReturnsNoContent()
    {
        var request = new
        {
            Email = "test@example.com",
            PhoneNumber = "0712345678",
            PasswordHash = "hash123",
            Username = "testuser",
            DiscountNotifications = true,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Patient"
        };

        var response = await client.PostAsJsonAsync("/api/Users", request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task CreateAndGetUser_ReturnsCreatedUser()
    {
        var request = new
        {
            Email = "john@example.com",
            PhoneNumber = "0711111111",
            PasswordHash = "pass",
            Username = "john",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 10,
            Role = "Patient"
        };

        await client.PostAsJsonAsync("/api/Users", request);
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        Assert.That(users!.Any(u => u.Username == "john"), Is.True);
    }

    [Test]
    public async Task GetUserById_NonExistentId_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Users/9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetUserExistsById_NonExistentId_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Users/9999/exists");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetUserByEmail_MissingParam_ReturnsBadRequest()
    {
        var response = await client.GetAsync("/api/Users/by-email");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetUserByEmail_NonExistentEmail_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/Users/by-email?email=nobody@test.com");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetUserExistsByEmail_MissingParam_ReturnsBadRequest()
    {
        var response = await client.GetAsync("/api/Users/exists");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetUserHasPeriodTracker_NonExistentUser_ReturnsFalse()
    {
        var result = await client.GetFromJsonAsync<bool>("/api/Users/9999/period-tracker");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CreateUser_ThenGetById_ReturnsOk()
    {
        var request = new
        {
            Email = "getbyid@example.com",
            PhoneNumber = "0700000001",
            PasswordHash = "h",
            Username = "getbyiduser",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Patient"
        };
        await client.PostAsJsonAsync("/api/Users", request);
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        var id = users!.First().Id;
        var response = await client.GetAsync($"/api/Users/{id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateUser_ThenExistsById_ReturnsTrue()
    {
        var request = new
        {
            Email = "exists@example.com",
            PhoneNumber = "0700000002",
            PasswordHash = "h",
            Username = "existsuser",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 5,
            Role = "Patient"
        };
        await client.PostAsJsonAsync("/api/Users", request);
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        var id = users!.First().Id;
        var result = await client.GetFromJsonAsync<bool>($"/api/Users/{id}/exists");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CreateUser_ThenGetByEmail_ReturnsOk()
    {
        var request = new
        {
            Email = "findme@example.com",
            PhoneNumber = "0700000003",
            PasswordHash = "h",
            Username = "findmeuser",
            DiscountNotifications = true,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Patient"
        };
        await client.PostAsJsonAsync("/api/Users", request);
        var response = await client.GetAsync("/api/Users/by-email?email=findme@example.com");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task CreateUser_ThenExistsByEmail_ReturnsTrue()
    {
        var request = new
        {
            Email = "emailexists@example.com",
            PhoneNumber = "0700000004",
            PasswordHash = "h",
            Username = "emailexists",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Patient"
        };
        await client.PostAsJsonAsync("/api/Users", request);
        var result = await client.GetFromJsonAsync<bool>("/api/Users/exists?email=emailexists@example.com");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task CreateUser_ThenUpdate_ReturnsNoContent()
    {
        var request = new
        {
            Email = "updateme@example.com",
            PhoneNumber = "0700000005",
            PasswordHash = "h",
            Username = "updateuser",
            DiscountNotifications = false,
            IsDisabled = false,
            IsAdmin = false,
            LoyaltyPoints = 0,
            Role = "Patient"
        };
        await client.PostAsJsonAsync("/api/Users", request);
        var users = await client.GetFromJsonAsync<List<User>>("/api/Users");
        var user = users!.First();
        user.LoyaltyPoints = 100;
        var response = await client.PutAsJsonAsync($"/api/Users/{user.Id}", user);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}

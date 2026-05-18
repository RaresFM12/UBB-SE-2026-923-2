using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UBB_SE_2026_923_2.IntegrationTests;

[TestFixture]
public class ItemsWebControllerIntegrationTests
{
    private const string ItemName = "Aspirin";
    private const string ItemProducer = "Bayer";
    private const string ItemCategory = "Pain Relief";
    private const string ItemPrice = "10.99";
    private const string ItemNumberOfPills = "30";
    private const string ItemQuantity = "100";
    private const string ItemLabel = "OTC";
    private const string ItemDescription = "Pain reliever";
    private const string ItemImagePath = "/img/aspirin.png";
    private const string ItemDiscountPercentage = "0";
    private const string ItemSubstancesText = "AcetylsalicylicAcid:500";
    private const string ItemBatchesText = "2027-12-31:50";

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
        HttpResponseMessage response = await this.httpClient.GetAsync("/Items");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_WhenCalledWithValidItem_RedirectsToIndex()
    {
        HttpResponseMessage getResponse = await this.httpClient.GetAsync("/Items/Create");
        string html = await getResponse.Content.ReadAsStringAsync();
        string token = AdminWebApplicationFactory.ExtractAntiForgeryToken(html);

        var formData = new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Name"] = ItemName,
            ["Producer"] = ItemProducer,
            ["Category"] = ItemCategory,
            ["Price"] = ItemPrice,
            ["NumberOfPills"] = ItemNumberOfPills,
            ["Quantity"] = ItemQuantity,
            ["Label"] = ItemLabel,
            ["Description"] = ItemDescription,
            ["ImagePath"] = ItemImagePath,
            ["DiscountPercentage"] = ItemDiscountPercentage,
            ["SubstancesText"] = ItemSubstancesText,
            ["BatchesText"] = ItemBatchesText,
        };

        HttpResponseMessage postResponse = await this.httpClient.PostAsync("/Items/Create", new FormUrlEncodedContent(formData));

        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(postResponse.Headers.Location?.ToString(), Does.Contain("/Items"));
    }
}

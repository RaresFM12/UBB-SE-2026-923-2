namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemsRepository repository;

    public ItemsController(IItemsRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Item>> GetAll([FromQuery] string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return this.Ok(this.repository.GetItemsByName(name));
        }

        return this.Ok(this.repository.GetAllItems());
    }

    [HttpGet("{itemId:int}")]
    public ActionResult<Item> GetById(int itemId)
    {
        if (!this.repository.ItemExists(itemId))
        {
            return this.NotFound();
        }

        return this.Ok(this.repository.GetItemById(itemId));
    }

    [HttpGet("{itemId:int}/exists")]
    public ActionResult<bool> Exists(int itemId)
    {
        return this.Ok(this.repository.ItemExists(itemId));
    }

    [HttpGet("top")]
    public ActionResult<List<ItemPopularitySummary>> GetTop()
    {
        var top = this.repository.GetTop30Items()
            .Select(topItem => new ItemPopularitySummary(topItem.Item1, topItem.Item2, topItem.Item3))
            .ToList();
        return this.Ok(top);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateItemRequest request)
    {
        this.repository.AddItem(
            request.Name,
            request.Producer,
            request.Category,
            request.Price,
            request.NumberOfPills,
            request.Label,
            request.Description,
            request.ImagePath,
            request.Discount);
        return this.NoContent();
    }

    [HttpPost("with-quantity")]
    public IActionResult CreateWithQuantity([FromBody] CreateItemWithQuantityRequest request)
    {
        this.repository.AddItemWithQuantity(
            request.Name,
            request.Producer,
            request.Category,
            request.Price,
            request.NumberOfPills,
            request.Quantity,
            request.ActiveSubstances,
            request.Batches,
            request.Label,
            request.Description,
            request.ImagePath,
            request.Discount);
        return this.NoContent();
    }

    [HttpPut("{itemId:int}")]
    public IActionResult Update(int itemId, [FromBody] Item item)
    {
        item.Id = itemId;
        this.repository.UpdateItemById(item);
        return this.NoContent();
    }

    [HttpDelete("{itemId:int}")]
    public IActionResult Delete(int itemId)
    {
        this.repository.RemoveItemById(itemId);
        return this.NoContent();
    }

    public record CreateItemRequest(
        string Name,
        string Producer,
        string Category,
        float Price,
        int NumberOfPills,
        string Label,
        string Description,
        string ImagePath,
        float Discount);

    public record CreateItemWithQuantityRequest(
        string Name,
        string Producer,
        string Category,
        float Price,
        int NumberOfPills,
        int Quantity,
        Dictionary<string, float> ActiveSubstances,
        Dictionary<DateOnly, int> Batches,
        string Label,
        string Description,
        string ImagePath,
        float Discount);
}
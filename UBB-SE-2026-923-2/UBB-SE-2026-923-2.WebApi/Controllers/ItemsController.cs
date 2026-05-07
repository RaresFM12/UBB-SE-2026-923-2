using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

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
            return Ok(repository.GetItemsByName(name));
        }

        return Ok(repository.GetAllItems());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Item> GetById(int id)
    {
        if (!repository.ItemExists(id))
        {
            return NotFound();
        }

        return Ok(repository.GetItemById(id));
    }

    [HttpGet("{id:int}/exists")]
    public ActionResult<bool> Exists(int id)
    {
        return Ok(repository.ItemExists(id));
    }

    [HttpGet("top")]
    public ActionResult<List<ItemPopularitySummary>> GetTop()
    {
        var top = repository.GetTop30Items()
            .Select(t => new ItemPopularitySummary(t.Item1, t.Item2, t.Item3))
            .ToList();
        return Ok(top);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateItemRequest request)
    {
        repository.AddItem(
            request.Name,
            request.Producer,
            request.Category,
            request.Price,
            request.NumberOfPills,
            request.Label,
            request.Description,
            request.ImagePath,
            request.Discount);
        return NoContent();
    }

    [HttpPost("with-quantity")]
    public IActionResult CreateWithQuantity([FromBody] CreateItemWithQuantityRequest request)
    {
        repository.AddItemWithQuantity(
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
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Item item)
    {
        item.Id = id;
        repository.UpdateItemById(item);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        repository.RemoveItemById(id);
        return NoContent();
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

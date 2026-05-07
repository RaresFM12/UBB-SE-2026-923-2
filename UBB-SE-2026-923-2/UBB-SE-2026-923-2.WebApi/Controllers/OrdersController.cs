using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersRepository repository;

    public OrdersController(IOrdersRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Order>> GetAll([FromQuery] int? clientId = null)
    {
        if (clientId.HasValue)
        {
            return Ok(repository.GetOrdersOfClient(clientId.Value));
        }

        return Ok(repository.GetAllOrders());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Order> GetById(int id)
    {
        if (!repository.OrderExists(id))
        {
            return NotFound();
        }

        return Ok(repository.GetOrder(id));
    }

    [HttpGet("{id:int}/exists")]
    public ActionResult<bool> Exists(int id)
    {
        return Ok(repository.OrderExists(id));
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateOrderRequest request)
    {
        var id = repository.AddOrder(request.ClientId, request.PickUpDate, request.IsCompleted, request.IsExpired);
        return Ok(id);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Order order)
    {
        order.Id = id;
        repository.UpdateOrder(order);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        repository.RemoveOrder(id);
        return NoContent();
    }

    public record CreateOrderRequest(int ClientId, DateOnly PickUpDate, bool IsCompleted, bool IsExpired);
}

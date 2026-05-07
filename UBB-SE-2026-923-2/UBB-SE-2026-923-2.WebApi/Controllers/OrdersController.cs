namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
            return this.Ok(this.repository.GetOrdersOfClient(clientId.Value));
        }

        return this.Ok(this.repository.GetAllOrders());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Order> GetById(int id)
    {
        if (!this.repository.OrderExists(id))
        {
            return this.NotFound();
        }

        return this.Ok(this.repository.GetOrder(id));
    }

    [HttpGet("{id:int}/exists")]
    public ActionResult<bool> Exists(int id)
    {
        return this.Ok(this.repository.OrderExists(id));
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateOrderRequest request)
    {
        var id = this.repository.AddOrder(request.ClientId, request.PickUpDate, request.IsCompleted, request.IsExpired);
        return this.Ok(id);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] Order order)
    {
        order.Id = id;
        this.repository.UpdateOrder(order);
        return this.NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        this.repository.RemoveOrder(id);
        return this.NoContent();
    }

    public record CreateOrderRequest(int ClientId, DateOnly PickUpDate, bool IsCompleted, bool IsExpired);
}

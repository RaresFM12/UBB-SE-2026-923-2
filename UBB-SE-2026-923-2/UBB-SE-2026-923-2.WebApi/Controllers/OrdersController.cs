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

    [HttpGet("{orderId:int}")]
    public ActionResult<Order> GetById(int orderId)
    {
        if (!this.repository.OrderExists(orderId))
        {
            return this.NotFound();
        }

        return this.Ok(this.repository.GetOrder(orderId));
    }

    [HttpGet("{orderId:int}/exists")]
    public ActionResult<bool> Exists(int orderId)
    {
        return this.Ok(this.repository.OrderExists(orderId));
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateOrderRequest request)
    {
        var id = this.repository.AddOrder(request.ClientId, request.PickUpDate, request.IsCompleted, request.IsExpired);
        return this.Ok(id);
    }

    [HttpPut("{orderId:int}")]
    public IActionResult Update(int orderId, [FromBody] Order order)
    {
        order.Id = orderId;
        this.repository.UpdateOrder(order);
        return this.NoContent();
    }

    [HttpDelete("{orderId:int}")]
    public IActionResult Delete(int orderId)
    {
        this.repository.RemoveOrder(orderId);
        return this.NoContent();
    }

    public record CreateOrderRequest(int ClientId, DateOnly PickUpDate, bool IsCompleted, bool IsExpired);
}
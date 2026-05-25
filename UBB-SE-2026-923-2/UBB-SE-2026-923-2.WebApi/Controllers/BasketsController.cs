namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class BasketsController : ControllerBase
{
    private readonly IBasketRepository repository;

    public BasketsController(IBasketRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet("{userId:int}")]
    public ActionResult<Dictionary<int, BasketEntry>> GetBasket(int userId)
    {
        return this.Ok(this.repository.GetBasket(userId));
    }

    [HttpPut("{userId:int}")]
    public IActionResult SaveBasket(int userId, [FromBody] Dictionary<int, BasketEntry>? basket)
    {
        this.repository.SaveBasket(userId, basket ?? new Dictionary<int, BasketEntry>());
        return this.NoContent();
    }

    [HttpDelete("{userId:int}")]
    public IActionResult ClearBasket(int userId)
    {
        this.repository.ClearBasket(userId);
        return this.NoContent();
    }
}

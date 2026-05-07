namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class ShiftSwapsController : ControllerBase
{
    private readonly IShiftSwapRepository repository;

    public ShiftSwapsController(IShiftSwapRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ShiftSwapRequest>> GetAll()
    {
        return this.Ok(this.repository.GetAllShiftSwapRequests());
    }

    [HttpGet("{id:int}")]
    public ActionResult<ShiftSwapRequest> GetById(int id)
    {
        var swap = this.repository.GetShiftSwapRequestById(id);
        if (swap is null)
        {
            return this.NotFound();
        }

        return this.Ok(swap);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] ShiftSwapRequest request)
    {
        var id = this.repository.AddShiftSwapRequest(request);
        return this.Ok(id);
    }

    [HttpPatch("{id:int}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        this.repository.UpdateShiftSwapRequestStatus(id, request.Status);
        return this.NoContent();
    }

    public record UpdateStatusRequest(string Status);
}

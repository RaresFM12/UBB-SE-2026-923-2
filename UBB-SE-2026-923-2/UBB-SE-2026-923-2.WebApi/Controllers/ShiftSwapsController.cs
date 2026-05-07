using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

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
        return Ok(repository.GetAllShiftSwapRequests());
    }

    [HttpGet("{id:int}")]
    public ActionResult<ShiftSwapRequest> GetById(int id)
    {
        var swap = repository.GetShiftSwapRequestById(id);
        if (swap is null)
        {
            return NotFound();
        }

        return Ok(swap);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] ShiftSwapRequest request)
    {
        var id = repository.AddShiftSwapRequest(request);
        return Ok(id);
    }

    [HttpPatch("{id:int}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        repository.UpdateShiftSwapRequestStatus(id, request.Status);
        return NoContent();
    }

    public record UpdateStatusRequest(string Status);
}

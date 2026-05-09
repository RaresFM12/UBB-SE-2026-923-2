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

    [HttpGet("{swapId:int}")]
    public ActionResult<ShiftSwapRequest> GetById(int swapId)
    {
        var swap = this.repository.GetShiftSwapRequestById(swapId);
        if (swap is null)
        {
            return this.NotFound();
        }

        return this.Ok(swap);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateShiftSwapRequest request)
    {
        var shiftSwapRequest = new ShiftSwapRequest
        {
            Shift = new Shift { Id = request.ShiftId },
            Requester = new Staff { StaffID = request.RequesterId },
            Colleague = new Staff { StaffID = request.ColleagueId },
            RequestedAt = request.RequestedAt,
            Status = request.Status,
        };
        var id = this.repository.AddShiftSwapRequest(shiftSwapRequest);
        return this.Ok(id);
    }

    [HttpPatch("{swapId:int}/status")]
    public IActionResult UpdateStatus(int swapId, [FromBody] UpdateStatusRequest request)
    {
        this.repository.UpdateShiftSwapRequestStatus(swapId, request.Status);
        return this.NoContent();
    }

    public record UpdateStatusRequest(string Status);

    public record CreateShiftSwapRequest(int ShiftId, int RequesterId, int ColleagueId, DateTime RequestedAt, ShiftSwapRequestStatus Status);
}
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
        var requests = repository.GetAllShiftSwapRequests();
        var result = requests.Select(r => new
        {
            r.SwapId,
            r.RequestedAt,
            r.Status,
            ShiftId = r.Shift.Id,
            RequesterId = r.Requester.StaffID,
            ColleagueId = r.Colleague.StaffID,
        });
        return Ok(result);
    }

    [HttpGet("{swapId:int}")]
    public IActionResult GetById(int swapId)
    {
        var swap = this.repository.GetShiftSwapRequestById(swapId);
        if (swap is null)
        {
            return this.NotFound();
        }

        return this.Ok(new
        {
            swap.SwapId,
            swap.RequestedAt,
            swap.Status,
            ShiftId = swap.Shift.Id,
            RequesterId = swap.Requester.StaffID,
            ColleagueId = swap.Colleague.StaffID,
        });
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateShiftSwapRequest request)
    {
        var requestedAt = request.RequestedAt == default ? DateTime.UtcNow : request.RequestedAt;
        var shiftSwapRequest = new ShiftSwapRequest
        {
            Shift = new Shift { Id = request.ShiftId },
            Requester = new Staff { StaffID = request.RequesterId },
            Colleague = new Staff { StaffID = request.ColleagueId },
            RequestedAt = requestedAt,
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

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HangoutParticipantsController : ControllerBase
{
    private readonly IHangoutParticipantRepository repository;

    public HangoutParticipantsController(IHangoutParticipantRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<HangoutParticipantSummary>> GetAll()
    {
        var participants = repository.GetAllParticipants()
            .Select(p => new HangoutParticipantSummary(p.HangoutId, p.StaffId))
            .ToList();
        return Ok(participants);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateParticipantRequest request)
    {
        repository.AddParticipant(request.HangoutId, request.StaffId);
        return NoContent();
    }

    public record CreateParticipantRequest(int HangoutId, int StaffId);
}

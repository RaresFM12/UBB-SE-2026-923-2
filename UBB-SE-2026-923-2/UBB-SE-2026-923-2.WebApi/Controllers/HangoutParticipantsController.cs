namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        var participants = this.repository.GetAllParticipants()
            .Select(participant => new HangoutParticipantSummary(participant.HangoutId, participant.StaffId))
            .ToList();
        return this.Ok(participants);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateParticipantRequest request)
    {
        this.repository.AddParticipant(request.HangoutId, request.StaffId);
        return this.NoContent();
    }

    public record CreateParticipantRequest(int HangoutId, int StaffId);
}
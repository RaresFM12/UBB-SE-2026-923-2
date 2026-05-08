namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class HangoutsController : ControllerBase
{
    private readonly IHangoutRepository repository;

    public HangoutsController(IHangoutRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Hangout>> GetAll()
    {
        return this.Ok(this.repository.GetAllHangouts());
    }

    [HttpGet("{hangoutId:int}")]
    public ActionResult<Hangout> GetById(int hangoutId)
    {
        var hangout = this.repository.GetHangoutById(hangoutId);
        if (hangout is null)
        {
            return this.NotFound();
        }

        return this.Ok(hangout);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateHangoutRequest request)
    {
        var id = this.repository.AddHangout(
            request.Title,
            request.Description,
            request.Date,
            request.MaxParticipants);
        return this.Ok(id);
    }

    public record CreateHangoutRequest(string Title, string Description, DateTime Date, int MaxParticipants);
}
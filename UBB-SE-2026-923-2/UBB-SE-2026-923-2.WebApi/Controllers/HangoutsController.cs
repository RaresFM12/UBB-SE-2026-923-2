using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

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
        return Ok(repository.GetAllHangouts());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Hangout> GetById(int id)
    {
        var hangout = repository.GetHangoutById(id);
        if (hangout is null)
        {
            return NotFound();
        }

        return Ok(hangout);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateHangoutRequest request)
    {
        var id = repository.AddHangout(
            request.Title,
            request.Description,
            request.Date,
            request.MaxParticipants);
        return Ok(id);
    }

    public record CreateHangoutRequest(string Title, string Description, DateTime Date, int MaxParticipants);
}

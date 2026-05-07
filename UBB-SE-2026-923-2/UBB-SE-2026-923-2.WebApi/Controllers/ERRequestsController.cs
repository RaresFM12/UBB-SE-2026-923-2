using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ERRequestsController : ControllerBase
{
    private readonly IERDispatchRepository repository;

    public ERRequestsController(IERDispatchRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ERRequest>> GetAll()
    {
        return Ok(repository.GetAllRequests());
    }

    [HttpGet("{id:int}")]
    public ActionResult<ERRequest> GetById(int id)
    {
        var request = repository.GetRequestById(id);
        if (request is null)
        {
            return NotFound();
        }

        return Ok(request);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateRequest request)
    {
        var id = repository.AddRequest(request.Specialization, request.Location, request.Status);
        return Ok(id);
    }

    [HttpPatch("{id:int}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        repository.UpdateRequestStatus(id, request.Status, request.AssignedDoctorId, request.AssignedDoctorName);
        return NoContent();
    }

    public record CreateRequest(string Specialization, string Location, string Status);

    public record UpdateStatusRequest(string Status, int? AssignedDoctorId, string? AssignedDoctorName);
}

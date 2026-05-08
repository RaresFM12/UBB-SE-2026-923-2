namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        return this.Ok(this.repository.GetAllRequests());
    }

    [HttpGet("{requestId:int}")]
    public ActionResult<ERRequest> GetById(int requestId)
    {
        var request = this.repository.GetRequestById(requestId);
        if (request is null)
        {
            return this.NotFound();
        }

        return this.Ok(request);
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CreateRequest request)
    {
        var id = this.repository.AddRequest(request.Specialization, request.Location, request.Status);
        return this.Ok(id);
    }

    [HttpPatch("{requestId:int}/status")]
    public IActionResult UpdateStatus(int requestId, [FromBody] UpdateStatusRequest request)
    {
        this.repository.UpdateRequestStatus(requestId, request.Status, request.AssignedDoctorId, request.AssignedDoctorName);
        return this.NoContent();
    }

    public record CreateRequest(string Specialization, string Location, string Status);

    public record UpdateStatusRequest(string Status, int? AssignedDoctorId, string? AssignedDoctorName);
}
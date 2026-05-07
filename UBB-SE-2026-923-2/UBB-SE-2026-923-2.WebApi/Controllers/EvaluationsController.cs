using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EvaluationsController : ControllerBase
{
    private readonly IEvaluationsRepository repository;

    public EvaluationsController(IEvaluationsRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<MedicalEvaluation>> GetAll()
    {
        return Ok(repository.GetAllEvaluations());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateEvaluationRequest request)
    {
        repository.AddEvaluation(
            request.DoctorId,
            request.PatientId,
            request.Diagnosis,
            request.Notes,
            request.Medications,
            request.AssumedRisk);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UpdateEvaluationRequest request)
    {
        repository.UpdateEvaluation(id, request.Diagnosis, request.Notes, request.Medications);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        repository.DeleteEvaluation(id);
        return NoContent();
    }

    public record CreateEvaluationRequest(
        int DoctorId,
        int PatientId,
        string Diagnosis,
        string Notes,
        string Medications,
        bool AssumedRisk);

    public record UpdateEvaluationRequest(string Diagnosis, string Notes, string Medications);
}

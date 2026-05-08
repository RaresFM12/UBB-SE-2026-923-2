namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        return this.Ok(this.repository.GetAllEvaluations());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateEvaluationRequest request)
    {
        this.repository.AddEvaluation(
            request.DoctorId,
            request.PatientId,
            request.Diagnosis,
            request.Notes,
            request.Medications,
            request.AssumedRisk);
        return this.NoContent();
    }

    [HttpPut("{evaluationId:int}")]
    public IActionResult Update(int evaluationId, [FromBody] UpdateEvaluationRequest request)
    {
        this.repository.UpdateEvaluation(evaluationId, request.Diagnosis, request.Notes, request.Medications);
        return this.NoContent();
    }

    [HttpDelete("{evaluationId:int}")]
    public IActionResult Delete(int evaluationId)
    {
        this.repository.DeleteEvaluation(evaluationId);
        return this.NoContent();
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
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HighRiskMedicinesController : ControllerBase
{
    private readonly IHighRiskMedicineRepository repository;

    public HighRiskMedicinesController(IHighRiskMedicineRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<HighRiskMedicineSummary>> GetAll()
    {
        var medicines = repository.GetAllHighRiskMedicines()
            .Select(m => new HighRiskMedicineSummary(m.MedicineName, m.WarningMessage))
            .ToList();
        return Ok(medicines);
    }
}

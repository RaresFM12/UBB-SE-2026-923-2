namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        var medicines = this.repository.GetAllHighRiskMedicines()
            .Select(medicine => new HighRiskMedicineSummary(medicine.MedicineName, medicine.WarningMessage))
            .ToList();
        return this.Ok(medicines);
    }
}
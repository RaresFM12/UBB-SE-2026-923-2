namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

[ApiController]
[Route("api/[controller]")]
public class SubstancesController : ControllerBase
{
    private readonly ISubstancesRepository repository;

    public SubstancesController(ISubstancesRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<List<Substance>> GetAll()
    {
        return this.Ok(this.repository.GetAllSubstances());
    }

    [HttpGet("{name}")]
    public ActionResult<Substance> GetByName(string name)
    {
        if (!this.repository.SubstanceExists(name))
        {
            return this.NotFound();
        }

        return this.Ok(this.repository.GetSubstanceByName(name));
    }

    [HttpGet("{name}/exists")]
    public ActionResult<bool> Exists(string name)
    {
        return this.Ok(this.repository.SubstanceExists(name));
    }

    [HttpGet("top")]
    public ActionResult<Dictionary<string, int>> GetTop()
    {
        return this.Ok(this.repository.GetTop30Substances());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateSubstanceRequest request)
    {
        this.repository.AddSubstance(request.Name, request.LethalDose, request.Description);
        return this.NoContent();
    }

    [HttpPut("{name}")]
    public IActionResult Update(string name, [FromBody] Substance substance)
    {
        substance.Name = name;
        this.repository.UpdateSubstanceByName(substance);
        return this.NoContent();
    }

    [HttpDelete("{name}")]
    public IActionResult Delete(string name)
    {
        this.repository.RemoveSubstanceByName(name);
        return this.NoContent();
    }

    public record CreateSubstanceRequest(string Name, float LethalDose, string Description);
}

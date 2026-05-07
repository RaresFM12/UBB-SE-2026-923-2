using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

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
        return Ok(repository.GetAllSubstances());
    }

    [HttpGet("{name}")]
    public ActionResult<Substance> GetByName(string name)
    {
        if (!repository.SubstanceExists(name))
        {
            return NotFound();
        }

        return Ok(repository.GetSubstanceByName(name));
    }

    [HttpGet("{name}/exists")]
    public ActionResult<bool> Exists(string name)
    {
        return Ok(repository.SubstanceExists(name));
    }

    [HttpGet("top")]
    public ActionResult<Dictionary<string, int>> GetTop()
    {
        return Ok(repository.GetTop30Substances());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateSubstanceRequest request)
    {
        repository.AddSubstance(request.Name, request.LethalDose, request.Description);
        return NoContent();
    }

    [HttpPut("{name}")]
    public IActionResult Update(string name, [FromBody] Substance substance)
    {
        substance.Name = name;
        repository.UpdateSubstanceByName(substance);
        return NoContent();
    }

    [HttpDelete("{name}")]
    public IActionResult Delete(string name)
    {
        repository.RemoveSubstanceByName(name);
        return NoContent();
    }

    public record CreateSubstanceRequest(string Name, float LethalDose, string Description);
}

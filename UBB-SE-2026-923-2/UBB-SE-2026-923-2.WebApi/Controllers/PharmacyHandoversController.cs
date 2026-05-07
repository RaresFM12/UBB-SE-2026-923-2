using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PharmacyHandoversController : ControllerBase
{
    private readonly IPharmacyHandoverRepository repository;

    public PharmacyHandoversController(IPharmacyHandoverRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<PharmacyHandover>> GetAll()
    {
        return Ok(repository.GetAllPharmacyHandovers());
    }
}

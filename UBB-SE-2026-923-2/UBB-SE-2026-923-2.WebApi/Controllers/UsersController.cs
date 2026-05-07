using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUsersRepository repository;

    public UsersController(IUsersRepository repository)
    {
        this.repository = repository;
    }

    [HttpGet]
    public ActionResult<List<User>> GetAll()
    {
        return Ok(repository.GetAllUsers());
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetById(int id)
    {
        if (!repository.UserExists(id))
        {
            return NotFound();
        }

        var user = repository.GetUserById(id);
        return Ok(user);
    }

    [HttpGet("by-email")]
    public ActionResult<User> GetByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("email query parameter is required");
        }

        if (!repository.UserExists(email))
        {
            return NotFound();
        }

        var user = repository.GetUserByEmail(email);
        return Ok(user);
    }

    [HttpGet("{id:int}/exists")]
    public ActionResult<bool> ExistsById(int id)
    {
        return Ok(repository.UserExists(id));
    }

    [HttpGet("exists")]
    public ActionResult<bool> ExistsByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("email query parameter is required");
        }

        return Ok(repository.UserExists(email));
    }

    [HttpGet("{id:int}/period-tracker")]
    public ActionResult<bool> HasPeriodTracker(int id)
    {
        return Ok(repository.UserHasPeriodTracker(id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        repository.AddUser(
            request.Email,
            request.PhoneNumber,
            request.PasswordHash,
            request.Username,
            request.DiscountNotifications,
            request.IsDisabled,
            request.IsAdmin,
            request.LoyaltyPoints,
            request.Role);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] User user)
    {
        // Defend against id mismatch between URL and payload — URL wins.
        user.Id = id;
        repository.UpdateUser(user);
        return NoContent();
    }

    public record CreateUserRequest(
        string Email,
        string PhoneNumber,
        string PasswordHash,
        string Username,
        bool DiscountNotifications,
        bool IsDisabled,
        bool IsAdmin,
        int LoyaltyPoints,
        string Role);
}

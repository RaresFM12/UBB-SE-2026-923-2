namespace UBB_SE_2026_923_2.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Repositories;

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
        return this.Ok(this.repository.GetAllUsers());
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetById(int id)
    {
        if (!this.repository.UserExists(id))
        {
            return this.NotFound();
        }

        var user = this.repository.GetUserById(id);
        return this.Ok(user);
    }

    [HttpGet("by-email")]
    public ActionResult<User> GetByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return this.BadRequest("email query parameter is required");
        }

        if (!this.repository.UserExists(email))
        {
            return this.NotFound();
        }

        var user = this.repository.GetUserByEmail(email);
        return this.Ok(user);
    }

    [HttpGet("{id:int}/exists")]
    public ActionResult<bool> ExistsById(int id)
    {
        return this.Ok(this.repository.UserExists(id));
    }

    [HttpGet("exists")]
    public ActionResult<bool> ExistsByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return this.BadRequest("email query parameter is required");
        }

        return this.Ok(this.repository.UserExists(email));
    }

    [HttpGet("{id:int}/period-tracker")]
    public ActionResult<bool> HasPeriodTracker(int id)
    {
        return this.Ok(this.repository.UserHasPeriodTracker(id));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUserRequest request)
    {
        this.repository.AddUser(
            request.Email,
            request.PhoneNumber,
            request.PasswordHash,
            request.Username,
            request.DiscountNotifications,
            request.IsDisabled,
            request.IsAdmin,
            request.LoyaltyPoints,
            request.Role);
        return this.NoContent();
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] User user)
    {
        // Defend against id mismatch between URL and payload — URL wins.
        user.Id = id;
        this.repository.UpdateUser(user);
        return this.NoContent();
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

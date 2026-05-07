using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository repository;

    public NotificationsController(INotificationRepository repository)
    {
        this.repository = repository;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateNotificationRequest request)
    {
        repository.AddNotification(request.RecipientStaffId, request.Title, request.Message);
        return NoContent();
    }

    public record CreateNotificationRequest(int RecipientStaffId, string Title, string Message);
}

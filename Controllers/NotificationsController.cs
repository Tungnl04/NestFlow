using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.Services;
using System.Security.Claims;

namespace NestFlow.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: api/Notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        // TODO: Ensure Authorize attribute is used in production or assume middleware handles user context
        // For now, allow passing userId via query for testing if auth is not fully ready, 
        // OR try to get from User claims if available.
        
        // Setup simple logic: try get claim, else requires query param for Dev/Testing if Auth not fully integrated
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) && Request.Query.ContainsKey("userId"))
        {
            userIdStr = Request.Query["userId"];
        }

        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out long userId))
        {
            return Unauthorized("User ID not found.");
        }

        var notifs = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifs);
    }

    // PUT: api/Notifications/5/read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok();
    }

    // PUT: api/Notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] long userId)
    {
        // If userId is not provided in query, try to get from Claims
        if (userId <= 0)
        {
             var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
             if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out long parsedId))
             {
                 userId = parsedId;
             }
        }

        if (userId <= 0) {
            return BadRequest("User ID required");
        }

        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
    }
    
    // POST: api/Notifications/test (For Dev Testing Only)
    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification([FromQuery] long userId, [FromBody] TestNotificationRequest request)
    {
        // DB only allows: 'booking', 'payment', 'message', 'review'
        await _notificationService.CreateAndSendNotificationAsync(userId, request.Title, request.Content, "message");
        return Ok("Sent");
    }

    public class TestNotificationRequest 
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

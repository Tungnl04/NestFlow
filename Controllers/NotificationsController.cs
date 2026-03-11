using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.Services.Interfaces;
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

    // Helper: lấy userId từ Session hoặc Claims
    private long? GetCurrentUserId()
    {
        // 1. Session
        var sessionId = HttpContext.Session.GetInt32("UserId");
        if (sessionId.HasValue) return sessionId.Value;

        // 2. Claims
        var claimStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(claimStr) && long.TryParse(claimStr, out long claimId)) return claimId;

        // 3. Query (dev/test)
        if (Request.Query.ContainsKey("userId") && long.TryParse(Request.Query["userId"], out long qId)) return qId;

        return null;
    }

    // GET: api/Notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User ID not found.");

        var notifs = await _notificationService.GetUserNotificationsAsync(userId.Value);
        return Ok(notifs);
    }

    // GET: api/Notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized("User ID not found.");

        var count = await _notificationService.GetUnreadCountAsync(userId.Value);
        return Ok(new { count });
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
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return BadRequest("User ID required");

        await _notificationService.MarkAllAsReadAsync(userId.Value);
        return Ok();
    }
    
    // POST: api/Notifications/test (For Dev Testing Only)
    [HttpPost("test")]
    public async Task<IActionResult> SendTestNotification([FromQuery] long userId, [FromBody] TestNotificationRequest request)
    {
        await _notificationService.CreateAndSendNotificationAsync(userId, request.Title, request.Content, "message");
        return Ok("Sent");
    }

    public class TestNotificationRequest 
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NestFlow.Hubs;
using NestFlow.Models;
using NestFlow.Application.Services.Interfaces;

namespace NestFlow.Application.Services;

public class NotificationService : INotificationService
{
    private readonly NestFlowSystemContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(NestFlowSystemContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task CreateAndSendNotificationAsync(long userId, string title, string content, string type, string linkUrl = "")
    {
        // 1. Save to Database
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            LinkUrl = linkUrl,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // 2. Push Realtime via SignalR
        // Targeting the specific group created in OnConnectedAsync
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            notification.NotificationId,
            notification.Title,
            notification.Content,
            notification.Type,
            notification.LinkUrl,
            notification.CreatedAt
        });
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(long userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit last 50
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(long notificationId)
    {
        var notif = await _context.Notifications.FindAsync(notificationId);
        if (notif != null)
        {
            notif.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        var unreadNotifs = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead != true)
            .ToListAsync();

        if (unreadNotifs.Any())
        {
            foreach (var notif in unreadNotifs)
            {
                notif.IsRead = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}

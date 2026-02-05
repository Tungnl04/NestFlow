using NestFlow.Models;

namespace NestFlow.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAndSendNotificationAsync(long userId, string title, string content, string type, string linkUrl = "");
        Task<List<Notification>> GetUserNotificationsAsync(long userId);
        Task MarkAsReadAsync(long notificationId);
        Task MarkAllAsReadAsync(long userId);
    }
}

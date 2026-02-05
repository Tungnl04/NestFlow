using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NestFlow.Hubs;

public class NotificationHub : Hub
{
    // Map connections to users
    // In a real production app with load balancers, you'd use a Redis backplane
    // For this project scale, we can rely on UserIdentifier from Context if authenticated
    
    public override async Task OnConnectedAsync()
    {
        string? userId = null;

        // 1. Try get from Claims (Best practice)
        userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Fallback: Try get from Session or Query String (Dev/Test convenience)
        if (string.IsNullOrEmpty(userId))
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                // Try Session
                if (httpContext.Session != null)
                {
                    try 
                    {
                        var sessionUserId = httpContext.Session.GetInt32("UserId");
                        if (sessionUserId.HasValue) userId = sessionUserId.ToString();
                    }
                    catch { /* Ignore session errors */ }
                }

                // Try Query String
                if (string.IsNullOrEmpty(userId))
                {
                    userId = httpContext.Request.Query["userId"];
                }
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            // Store in Context.Items for retrieval in OnDisconnected
            Context.Items["UserId"] = userId;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            Console.WriteLine($"SignalR Connected: User {userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("UserId", out var userIdObj) && userIdObj is string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}

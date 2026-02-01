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
        // 1. Try get from Claims (if Cookie Auth was used)
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Fallback: Try get from Session (if HttpContext available)
        if (string.IsNullOrEmpty(userId))
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext?.Session != null)
            {
                // Note: Session might be null if not configured for SignalR path
                try
                {
                   var sessionUserId = httpContext.Session.GetInt32("UserId");
                   if (sessionUserId.HasValue)
                   {
                       userId = sessionUserId.Value.ToString();
                   }
                }
                catch { /* Ignore session error */ }
            }
        }

        // 3. Fallback: Try get from Query String (Common for SignalR clients)
        if (string.IsNullOrEmpty(userId))
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                 userId = httpContext.Request.Query["userId"];
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
             // Add user to a group named by their UserID for easy targeting
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            Console.WriteLine($"SignalR Connected: User {userId} - ID {Context.ConnectionId}");
        }
        else
        {
            Console.WriteLine($"SignalR Connected: Anonymous - ID {Context.ConnectionId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // We need to retrieve userId again to remove from group.
        // Context.User might work if it was set, but for QueryString/Session we might need to store it in Context.Items during OnConnected
        // For simple Group management, SignalR auto cleans up connectionId from groups on disconnect, 
        // effectively we don't strictly need to manually RemoveFromGroupAsync unless we are tracking presence.
        // But let's try to be clean.
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
             var httpContext = Context.GetHttpContext();
             if (httpContext != null)
             {
                 if (httpContext.Session != null)
                 {
                     var sId = httpContext.Session.GetInt32("UserId");
                     if(sId.HasValue) userId = sId.Value.ToString();
                 }
                 if(string.IsNullOrEmpty(userId))
                 {
                     userId = httpContext.Request.Query["userId"];
                 }
             }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}

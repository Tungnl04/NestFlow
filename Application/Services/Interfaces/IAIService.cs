namespace NestFlow.Application.Services.Interfaces
{
    public interface IAIService
    {
        Task<string> GenerateResponseAsync(string userMessage, List<Controllers.ChatMessage> history, bool isAuthenticated = false);
    }
}

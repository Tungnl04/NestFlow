using Microsoft.AspNetCore.Mvc;
using NestFlow.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIChatController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly NestFlow.Models.NestFlowSystemContext _context;

        public AIChatController(IAIService aiService, NestFlow.Models.NestFlowSystemContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        [HttpGet("contact/{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize] // Require Login
        public async Task<IActionResult> GetContactInfo(int id)
        {
            try 
            {
                var property = await _context.Properties
                    .Include(p => p.Landlord)
                    .FirstOrDefaultAsync(p => p.PropertyId == id);

                if (property == null)
                {
                    return NotFound(new { error = "Không tìm thấy phòng này." });
                }

                if (property.Landlord == null)
                {
                    return BadRequest(new { error = "Thông tin chủ nhà chưa được cập nhật." });
                }

                // TODO: INTEGRATE "TRACE LOGIC" HERE
                // Example: _trackingService.LogViewContact(userId, propertyId);
                // Console.WriteLine($"[TRACE] Customer viewed contact of Property {id}");

                return Ok(new 
                { 
                    phone = property.Landlord.Phone ?? "Chưa cập nhật", 
                    name = property.Landlord.FullName ?? "Chủ nhà"
                });
            }
            catch (Exception ex)
            {
                // Log the error (in production, use proper logging)
                Console.WriteLine($"Error in GetContactInfo: {ex.Message}");
                return StatusCode(500, new { error = "Đã có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Tin nhắn không được để trống");
            }

            // Check if user is authenticated
            bool isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            
            // Debug log
            Console.WriteLine($"[AI Chat] User authenticated: {isAuthenticated}, Name: {User.Identity?.Name ?? "Anonymous"}");

            // Call Real AI Service with auth status
            string reply = await _aiService.GenerateResponseAsync(request.Message, request.History, isAuthenticated);

            return Ok(new { reply });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public List<ChatMessage> History { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "model"
        public string Message { get; set; }
    }
}

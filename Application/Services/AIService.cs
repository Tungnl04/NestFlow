using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class AIService : IAIService
    {
        private readonly NestFlowSystemContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIService(NestFlowSystemContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<string> GenerateResponseAsync(string userMessage, List<Controllers.ChatMessage> history, bool isAuthenticated = false)
        {
            var apiKey = _configuration["Groq:ApiKey"];
            var model = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

            if (string.IsNullOrEmpty(apiKey))
            {
                return "Chưa cấu hình API Key cho AI.";
            }

            // 1. Fetch Context Data (RAG)
            var roomContext = await GetRoomContextAsync(isAuthenticated);
            
            // 2. Build System Prompt - Natural responses with detailed info
            var systemPrompt = $@"Bạn là trợ lý ảo thông minh của NestFlow - Nền tảng tìm kiếm phòng trọ tại khu vực Hòa Lạc.
Nhiệm vụ của bạn là CHỈ hỗ trợ người dùng tìm phòng trọ trên hệ thống NestFlow.
Hãy trả lời ngắn gọn, thân thiện và chuyên nghiệp bằng Tiếng Việt.

QUAN TRỌNG:
- TUYỆT ĐỐI KHÔNG trả lời các câu hỏi không liên quan đến thuê phòng, nhà đất, hoặc nội quy trọ.
- Nếu người dùng hỏi chuyện ngoài lề, hãy từ chối khéo: ""Xin lỗi, tôi chỉ là trợ lý ảo hỗ trợ tìm phòng trọ tại NestFlow.""

KHI GIỚI THIỆU PHÒNG - BẮT BUỘC HIỂN THỊ ĐẦY ĐỦ:
- **Format chuẩn cho MỖI PHÒNG:**
  1. Tên phòng (Mã: X) (IN ĐẬM)
  2. • Giá: X VNĐ (XUỐNG DÒNG)
  3. • Địa chỉ: Đầy đủ địa chỉ (XUỐNG DÒNG)
  4. • Diện tích: Xm2 (XUỐNG DÒNG)
  5. • Tiện nghi: Liệt kê đầy đủ (XUỐNG DÒNG)
  6. • Liên hệ: Tên chủ nhà (nếu hỏi về liên hệ, hướng dẫn xem trang chi tiết)

- **VÍ DỤ CHUẨN (MỖI DÒNG • PHẢI XUỐNG DÒNG):**
  ""Phòng trọ khép kín tại Thạch Hòa (Mã: 1)(IN ĐẬM CHỮ)
  • Giá: 2,500,000 VNĐ
  • Địa chỉ: Số 10, Thôn Cảnh Châu, Bình Yên
  • Diện tích: 25m2
  • Tiện nghi: Wifi, Máy lạnh, Nóng lạnh, Giường, Tủ lạnh
  • Liên hệ: Bạn có thể xem thông tin liên hệ chi tiết bằng cách xem trang chi tiết.
  
  (Để dòng trống giữa các phòng)""

- **LUÔN LUÔN** hiển thị đầy đủ 5 thông tin trên cho MỖI phòng được đề cập.
- Nếu liệt kê nhiều phòng, mỗi phòng MỘT ĐOẠN RIÊNG với DÒNG TRỐNG ở giữa.
- PHẢI có mã phòng dạng (Mã: X) để hệ thống tự động thêm nút xem chi tiết.
- **QUAN TRỌNG**: Mỗi dấu • phải XUỐNG DÒNG riêng, KHÔNG được viết liền.

VỀ LIÊN HỆ & SỐ ĐIỆN THOẠI:
- TUYỆT ĐỐI KHÔNG hiển thị số điện thoại trực tiếp.
- Hướng dẫn: ""Bạn có thể xem thông tin liên hệ chi tiết bằng cách xem trang chi tiết.""

VỀ TIỆN NGHI:
- Liệt kê ĐẦY ĐỦ tất cả tiện nghi có trong dữ liệu.
- Nếu không có, nói rõ: ""Chưa cập nhật thông tin tiện nghi.""

Dưới đây là danh sách các phòng trọ hiện có trong hệ thống (Dữ liệu thực tế):
{roomContext}

**QUY TẮC TRẢ LỜI:**
- Khi người dùng tìm phòng theo tiêu chí (giá, địa điểm, diện tích), hãy liệt kê TẤT CẢ phòng phù hợp với ĐẦY ĐỦ thông tin.
- Không nói chung chung kiểu ""có một số phòng"" mà phải liệt kê CỤ THỂ từng phòng.
- Mỗi phòng phải có đầy đủ: Mã, Giá, Địa chỉ, Diện tích, Tiện nghi.

Nếu không tìm thấy, nói thật và gợi ý liên hệ hotline.
Đừng bịa đặt thông tin.";

            // 3. Call Groq API (OpenAI-compatible)
            var url = "https://api.groq.com/openai/v1/chat/completions";

            // 4. Build Messages Array (OpenAI format)
            var messages = new List<object>();

            // Add System Message
            messages.Add(new
            {
                role = "system",
                content = systemPrompt
            });

            // Add History
            if (history != null)
            {
                foreach (var msg in history)
                {
                    messages.Add(new
                    {
                        role = msg.Role == "ai" ? "assistant" : "user",
                        content = msg.Message
                    });
                }
            }

            // Add Current Message
            messages.Add(new
            {
                role = "user",
                content = userMessage
            });

            var requestBody = new
            {
                model = model,
                messages = messages,
                temperature = 0.7,
                max_tokens = 1024
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();

                    // Handle Rate Limit (429)
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        try 
                        {
                            using var errorDoc = JsonDocument.Parse(errorDetails);
                            if (errorDoc.RootElement.TryGetProperty("error", out var errorEl) && 
                                errorEl.TryGetProperty("message", out var msgEl))
                            {
                                var errorMsg = msgEl.GetString();
                                if (errorMsg?.Contains("rate limit") == true || errorMsg?.Contains("quota") == true)
                                {
                                    return "Hệ thống AI đang quá tải. Vui lòng thử lại sau vài giây.";
                                }
                            }
                        }
                        catch 
                        {
                            // Fallback if parsing fails
                        }
                        return "Hệ thống AI đang quá tải do nhận quá nhiều yêu cầu. Vui lòng thử lại sau 30 giây.";
                    }

                    return $"Lỗi API: {response.StatusCode}. Chi tiết: {errorDetails}";
                }

                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return text ?? "Xin lỗi, tôi không hiểu câu hỏi.";
            }
            catch (Exception ex)
            {
                return $"Đã xảy ra lỗi: {ex.Message}";
            }
        }

        private async Task<string> GetRoomContextAsync(bool isAuthenticated = false)
        {
            // Fetch relevant data: active properties
            // Limit to top 20 newest or most relevant to keep context size manageable
            var properties = await _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.Amenities)
                .Where(p => p.Status == "available") // Assuming 'available' status
                .OrderByDescending(p => p.CreatedAt)
                .Take(15)
                .Select(p => new
                {
                    p.PropertyId,
                    p.Title,
                    p.Price,
                    p.Address,
                    p.Ward,
                    p.Area,
                    LandlordName = p.Landlord.FullName,
                    LandlordPhone = p.Landlord.Phone, // Get actual phone
                    AmenityNames = string.Join(", ", p.Amenities.Select(a => a.Name))
                })
                .ToListAsync();

            if (!properties.Any())
            {
                return "Hiện tại chưa có phòng nào trống trong hệ thống.";
            }

            var sb = new StringBuilder();
            foreach (var p in properties)
            {
                sb.AppendLine($"- Phòng: {p.Title} (Mã: {p.PropertyId})");
                sb.AppendLine($"  + Giá: {p.Price:N0} VNĐ");
                sb.AppendLine($"  + Địa chỉ: {p.Address}, {p.Ward}");
                sb.AppendLine($"  + Diện tích: {p.Area}m2");
                sb.AppendLine($"  + Tiện nghi: {p.AmenityNames}");
                sb.AppendLine($"  + Liên hệ: {p.LandlordName}");
                sb.AppendLine($"  + Chi tiết: [[DETAIL:{p.PropertyId}]]");
                sb.AppendLine("---");
            }
            return sb.ToString();
        }
    }
}

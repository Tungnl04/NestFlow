using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;
using NestFlow.Application.Services.Interfaces;
using System.Text;

namespace NestFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly NestFlowSystemContext _context;
        private readonly INotificationService? _notificationService;

        public InvoiceController(NestFlowSystemContext context, INotificationService? notificationService = null)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Lấy chi tiết hóa đơn
        /// </summary>
        [HttpGet("detail/{invoiceId}")]
        public async Task<IActionResult> GetInvoiceDetail(long invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Property)
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Renter)
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.RentalOccupants)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Lấy người thuê đang ở
                var occupant = invoice.Rental?.RentalOccupants?
                    .FirstOrDefault(o => o.Status == "active");

                var detail = new
                {
                    invoice.InvoiceId,
                    invoice.RentalId,

                    PropertyTitle = invoice.Rental?.Property?.Title,
                    PropertyAddress = invoice.Rental?.Property?.Address,

                    // Ưu tiên Users, nếu không có thì lấy RentalOccupants
                    RenterName = invoice.Rental?.Renter?.FullName ?? occupant?.FullName ?? "Chưa có người thuê",
                    RenterPhone = invoice.Rental?.Renter?.Phone ?? occupant?.Phone,
                    RenterEmail = invoice.Rental?.Renter?.Email,

                    invoice.InvoiceMonth,
                    invoice.DueDate,
                    invoice.PaymentDate,
                    invoice.PaymentMethod,
                    invoice.Status,
                    invoice.TotalAmount,
                    invoice.RoomRent,
                    invoice.ElectricAmount,
                    invoice.WaterAmount,
                    invoice.InternetFee,
                    invoice.ElectricUsage,
                    invoice.WaterUsage,
                    invoice.ElectricOldReading,
                    invoice.ElectricNewReading,
                    invoice.WaterOldReading,
                    invoice.WaterNewReading,
                    invoice.OtherFees,
                    invoice.Notes,
                    invoice.CreatedAt,
                    invoice.UpdatedAt,

                    IsOverdue = invoice.Status != "paid"
                        && invoice.DueDate.HasValue
                        && invoice.DueDate < DateOnly.FromDateTime(DateTime.Now)
                };

                return Ok(new { success = true, data = detail });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting invoice detail: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách hóa đơn của landlord
        /// </summary>
        [HttpGet("landlord/{landlordId}")]
        public async Task<IActionResult> GetLandlordInvoices(long landlordId, [FromQuery] string? status = null)
        {
            try
            {
                var query = _context.Invoices
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Property)
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Renter)
                    .Include(i => i.Rental)                  // ← thêm
                        .ThenInclude(r => r.RentalOccupants) // ← thêm
                    .Where(i => i.Rental.LandlordId == landlordId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(i => i.Status == status);
                }

                var invoices = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                var result = invoices.Select(i =>
                {
                    var occupantName = i.Rental?.RentalOccupants?
                        .FirstOrDefault(o => o.Status == "active")?.FullName;

                    return new
                    {
                        i.InvoiceId,
                        i.RentalId,
                        PropertyTitle = i.Rental?.Property?.Title,
                        RenterName = i.Rental?.Renter?.FullName
                                  ?? occupantName
                                  ?? "Chưa có người thuê", // ← fallback
                        i.InvoiceMonth,
                        i.DueDate,
                        i.PaymentDate,
                        i.Status,
                        i.TotalAmount,
                        i.RoomRent,
                        i.ElectricAmount,
                        i.WaterAmount,
                        i.InternetFee,
                        i.OtherFees,
                        i.Notes,
                        i.ElectricUsage,
                        i.WaterUsage,
                        IsOverdue = i.Status != "paid"
                            && i.DueDate.HasValue
                            && i.DueDate < DateOnly.FromDateTime(DateTime.Now)
                    };
                });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting invoices: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Tạo hóa đơn mới
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            try
            {
                // Kiểm tra rental
                var rental = await _context.Rentals
                    .Include(r => r.Property)
                    .FirstOrDefaultAsync(r => r.RentalId == request.RentalId);

                if (rental == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hợp đồng thuê" });
                }

                // Tính tổng tiền
                decimal totalAmount = (request.RoomRent ?? 0) +
                                     (request.ElectricAmount ?? 0) +
                                     (request.WaterAmount ?? 0) +
                                     (request.InternetFee ?? 0);

                var invoice = new Invoice
                {
                    RentalId = request.RentalId,
                    InvoiceMonth = request.InvoiceMonth,
                    DueDate = DateOnly.FromDateTime(request.DueDate),
                    Status = "pending",
                    TotalAmount = totalAmount,
                    RoomRent = request.RoomRent,
                    ElectricAmount = request.ElectricAmount,
                    WaterAmount = request.WaterAmount,
                    InternetFee = request.InternetFee,
                    ElectricUsage = request.ElectricUsage,
                    WaterUsage = request.WaterUsage,
                    ElectricOldReading = request.ElectricOldReading,
                    ElectricNewReading = request.ElectricNewReading,
                    WaterOldReading = request.WaterOldReading,
                    WaterNewReading = request.WaterNewReading,
                    OtherFees = request.OtherFees,
                    Notes = request.Notes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Gửi thông báo cho chủ trọ
                try
                {
                    if (_notificationService != null)
                    {
                        await _notificationService.CreateAndSendNotificationAsync(
                            rental.LandlordId,
                            "Tạo hóa đơn thành công",
                            $"Bạn đã tạo thành công hóa đơn tháng {request.InvoiceMonth} cho Phòng {rental.Property?.Title}: {totalAmount:N0} VNĐ",
                            "success",
                            $"/Landlord/Invoices"
                        );
                    }
                }
                catch (Exception notiEx)
                {
                    Console.WriteLine($"Error sending invoice notification: {notiEx.Message}");
                }

                return Ok(new
                {
                    success = true,
                    message = "Tạo hóa đơn thành công",
                    invoiceId = invoice.InvoiceId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating invoice: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật hóa đơn
        /// </summary>
        [HttpPut("update/{invoiceId}")]
        public async Task<IActionResult> UpdateInvoice(long invoiceId, [FromBody] UpdateInvoiceRequest request)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Chỉ cho phép cập nhật nếu chưa thanh toán
                if (invoice.Status == "paid")
                {
                    return BadRequest(new { success = false, message = "Không thể cập nhật hóa đơn đã thanh toán" });
                }

                invoice.RoomRent = request.RoomRent;
                invoice.ElectricAmount = request.ElectricAmount;
                invoice.WaterAmount = request.WaterAmount;
                invoice.InternetFee = request.InternetFee;
                invoice.ElectricUsage = request.ElectricUsage;
                invoice.WaterUsage = request.WaterUsage;
                invoice.ElectricOldReading = request.ElectricOldReading;
                invoice.ElectricNewReading = request.ElectricNewReading;
                invoice.WaterOldReading = request.WaterOldReading;
                invoice.WaterNewReading = request.WaterNewReading;
                invoice.OtherFees = request.OtherFees;
                invoice.Notes = request.Notes;
                invoice.DueDate = DateOnly.FromDateTime(request.DueDate);

                // Tính lại tổng tiền
                invoice.TotalAmount = (request.RoomRent ?? 0) +
                                     (request.ElectricAmount ?? 0) +
                                     (request.WaterAmount ?? 0) +
                                     (request.InternetFee ?? 0);

                invoice.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating invoice: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu hóa đơn đã thanh toán
        /// </summary>
        [HttpPost("mark-paid/{invoiceId}")]
        public async Task<IActionResult> MarkAsPaid(long invoiceId, [FromBody] MarkPaidRequest request)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                invoice.Status = "paid";
                invoice.PaymentDate = DateOnly.FromDateTime(request.PaidDate);
                invoice.PaymentMethod = request.PaymentMethod;
                invoice.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã đánh dấu hóa đơn đã thanh toán" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking invoice as paid: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa hóa đơn
        /// </summary>
        [HttpDelete("delete/{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(long invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Chỉ cho phép xóa nếu chưa thanh toán
                if (invoice.Status == "paid")
                {
                    return BadRequest(new { success = false, message = "Không thể xóa hóa đơn đã thanh toán" });
                }

                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Xóa hóa đơn thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting invoice: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái overdue cho các hóa đơn quá hạn
        /// </summary>
        [HttpPost("update-overdue")]
        public async Task<IActionResult> UpdateOverdueInvoices()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var overdueInvoices = await _context.Invoices
                    .Where(i => i.Status == "issued" && i.DueDate < today)
                    .ToListAsync();

                foreach (var invoice in overdueInvoices)
                {
                    invoice.Status = "overdue";
                    invoice.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Đã cập nhật {overdueInvoices.Count} hóa đơn quá hạn"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating overdue invoices: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Xuất báo cáo CSV (có thể mở bằng Excel)
        /// </summary>
        [HttpGet("export-csv/{landlordId}")]
        public async Task<IActionResult> ExportToCSV(
            long landlordId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = _context.Invoices
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Property)
                    .Include(i => i.Rental)
                        .ThenInclude(r => r.Renter)
                    .Where(i => i.Rental.LandlordId == landlordId);

                if (fromDate.HasValue && toDate.HasValue)
                {
                    var from = DateOnly.FromDateTime(fromDate.Value);
                    var to = DateOnly.FromDateTime(toDate.Value);
                    query = query.Where(i => i.DueDate >= from && i.DueDate <= to);
                }

                var invoices = await query
                    .OrderBy(i => i.CreatedAt)
                    .ToListAsync();

                var csv = new StringBuilder();
                
                // UTF-8 BOM for Excel
                csv.Append("\uFEFF");
                
                // Header
                csv.AppendLine("Mã HĐ,Tháng,Hạn thanh toán,Ngày thanh toán,Trạng thái,Phòng,Người thuê,Tiền thuê,Tiền điện,Tiền nước,Phí internet,Phí khác,Tổng tiền,Ghi chú");

                // Data
                foreach (var invoice in invoices)
                {
                    csv.AppendLine($"{invoice.InvoiceId}," +
                        $"{invoice.InvoiceMonth}," +
                        $"{(invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("dd/MM/yyyy") : "")}," +
                        $"{(invoice.PaymentDate.HasValue ? invoice.PaymentDate.Value.ToString("dd/MM/yyyy") : "")}," +
                        $"{GetStatusText(invoice.Status)}," +
                        $"\"{invoice.Rental.Property.Title}\"," +
                        $"\"{invoice.Rental.Renter.FullName}\"," +
                        $"{invoice.RoomRent ?? 0}," +
                        $"{invoice.ElectricAmount ?? 0}," +
                        $"{invoice.WaterAmount ?? 0}," +
                        $"{invoice.InternetFee ?? 0}," +
                        $"\"{invoice.OtherFees?.Replace("\"", "\"\"")}\"," +
                        $"{invoice.TotalAmount ?? 0}," +
                        $"\"{invoice.Notes?.Replace("\"", "\"\"")}\"");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"BaoCaoHoaDon_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting CSV: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thống kê doanh thu
        /// </summary>
        [HttpGet("revenue-stats/{landlordId}")]
        public async Task<IActionResult> GetRevenueStats(
            long landlordId,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            try
            {
                var currentYear = year ?? DateTime.Now.Year;
                var query = _context.Invoices
                    .Include(i => i.Rental)
                    .Where(i => i.Rental.LandlordId == landlordId &&
                               i.CreatedAt.HasValue &&
                               i.CreatedAt.Value.Year == currentYear);

                if (month.HasValue)
                {
                    query = query.Where(i => i.CreatedAt.Value.Month == month.Value);
                }

                var invoices = await query.ToListAsync();

                var stats = new
                {
                    totalInvoices = invoices.Count,
                    paidInvoices = invoices.Count(i => i.Status == "paid"),
                    overdueInvoices = invoices.Count(i => i.Status == "overdue"),
                    pendingInvoices = invoices.Count(i => i.Status == "pending"),
                    totalRevenue = invoices.Where(i => i.Status == "paid").Sum(i => i.TotalAmount ?? 0),
                    pendingRevenue = invoices.Where(i => i.Status != "paid").Sum(i => i.TotalAmount ?? 0),
                    rentRevenue = invoices.Where(i => i.Status == "paid").Sum(i => i.RoomRent ?? 0),
                    electricityRevenue = invoices.Where(i => i.Status == "paid").Sum(i => i.ElectricAmount ?? 0),
                    waterRevenue = invoices.Where(i => i.Status == "paid").Sum(i => i.WaterAmount ?? 0),
                    internetRevenue = invoices.Where(i => i.Status == "paid").Sum(i => i.InternetFee ?? 0)
                };

                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting revenue stats: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        /// <summary>
        /// Lấy danh sách nhà trọ của landlord (dùng cho dropdown tạo hóa đơn)
        /// </summary>
        [HttpGet("buildings/{landlordId}")]
        public async Task<IActionResult> GetBuildingsForInvoice(long landlordId)
        {
            try
            {
                var buildings = await _context.Buildings
                    .Where(b => b.LandlordId == landlordId)
                    .Select(b => new
                    {
                        b.BuildingId,
                        b.BuildingName,
                        FullAddress = (b.Address ?? "") + ", " + (b.District ?? "") + ", " + (b.City ?? "")
                    })
                    .OrderBy(b => b.BuildingName)
                    .ToListAsync();

                return Ok(new { success = true, data = buildings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng đang có hợp đồng active theo building (dùng cho dropdown tạo hóa đơn)
        /// </summary>
        [HttpGet("occupied-rooms/{buildingId}")]
        public async Task<IActionResult> GetOccupiedRoomsByBuilding(long buildingId)
        {
            try
            {
                var rooms = await _context.Rentals
                    .Include(r => r.Property)
                    .Include(r => r.RentalOccupants)
                    .Where(r => r.Property.BuildingId == buildingId && r.Status == "active")
                    .Select(r => new
                    {
                        r.RentalId,
                        PropertyTitle = r.Property.Title,
                        RoomNumber = r.Property.RoomNumber,
                        RenterName = r.RentalOccupants
                            .Where(o => o.Status == "active")
                            .Select(o => o.FullName)
                            .FirstOrDefault(),
                        r.MonthlyRent
                    })
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync();

                return Ok(new { success = true, data = rooms });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private string GetStatusText(string? status)
        {
            return status switch
            {
                "pending" => "Chờ thanh toán",
                "paid" => "Đã thanh toán",
                "overdue" => "Quá hạn",
                "cancelled" => "Đã hủy",
                _ => status ?? ""
            };
        }
    }

    // Request models
    public class CreateInvoiceRequest
    {
        public long RentalId { get; set; }
        public string? InvoiceMonth { get; set; }
        public DateTime DueDate { get; set; }
        public decimal? RoomRent { get; set; }
        public decimal? ElectricAmount { get; set; }
        public decimal? WaterAmount { get; set; }
        public decimal? InternetFee { get; set; }
        public int? ElectricUsage { get; set; }
        public int? WaterUsage { get; set; }
        public int? ElectricOldReading { get; set; }
        public int? ElectricNewReading { get; set; }
        public int? WaterOldReading { get; set; }
        public int? WaterNewReading { get; set; }
        public string? OtherFees { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateInvoiceRequest
    {
        public decimal? RoomRent { get; set; }
        public decimal? ElectricAmount { get; set; }
        public decimal? WaterAmount { get; set; }
        public decimal? InternetFee { get; set; }
        public int? ElectricUsage { get; set; }
        public int? WaterUsage { get; set; }
        public int? ElectricOldReading { get; set; }
        public int? ElectricNewReading { get; set; }
        public int? WaterOldReading { get; set; }
        public int? WaterNewReading { get; set; }
        public string? OtherFees { get; set; }
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class MarkPaidRequest
    {
        public DateTime PaidDate { get; set; }
        public string PaymentMethod { get; set; } = "cash";
    }
}

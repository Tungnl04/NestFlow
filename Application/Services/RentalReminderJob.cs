using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services;

/// <summary>
/// Background Job: Nhắc hạn thanh toán và hợp đồng thuê
/// Chạy mỗi ngày lúc 8h sáng
/// </summary>
public class RentalReminderJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RentalReminderJob> _logger;

    public RentalReminderJob(IServiceScopeFactory scopeFactory, ILogger<RentalReminderJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RentalReminderJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                // Chạy lúc 8h sáng mỗi ngày
                var nextRun = now.Date.AddHours(8);
                if (now > nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                _logger.LogInformation($"RentalReminderJob: Lần chạy tiếp theo lúc {nextRun:dd/MM/yyyy HH:mm}");

                await Task.Delay(delay, stoppingToken);

                await RunReminders(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"RentalReminderJob lỗi: {ex.Message}");
                // Chờ 1 phút rồi thử lại
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task RunReminders(CancellationToken ct)
    {
        _logger.LogInformation("=== RentalReminderJob: Bắt đầu kiểm tra nhắc hạn ===");

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NestFlowSystemContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateOnly.FromDateTime(DateTime.Now);

        // 1. Cảnh báo quá hạn thanh toán (chỉ cho chủ trọ)
        await WarnOverduePayments(context, notificationService, today);

        // 2. Nhắc hợp đồng sắp hết hạn (Rental.EndDate trong 7 ngày tới, chỉ cho chủ trọ)
        await RemindExpiringContracts(context, notificationService, today);

        // 3. Nhắc hóa đơn đến hạn / quá hạn cho Chủ trọ
        await CheckInvoiceDueDates(context, notificationService, today);

        _logger.LogInformation("=== RentalReminderJob: Hoàn tất ===");
    }

    /// <summary>
    /// Cảnh báo quá hạn: RentSchedule đã quá DueDate + Status == "Unpaid"
    /// </summary>
    private async Task WarnOverduePayments(
        NestFlowSystemContext context,
        INotificationService notificationService,
        DateOnly today)
    {
        try
        {
            var overdueSchedules = await context.RentSchedules
                .Include(rs => rs.Rental)
                    .ThenInclude(r => r.Property)
                .Where(rs => rs.DueDate < today && rs.Status == "Unpaid")
                .ToListAsync();

            foreach (var schedule in overdueSchedules)
            {
                if (schedule.Rental?.RenterId == null) continue;

                var daysOverdue = today.DayNumber - schedule.DueDate.DayNumber;

                // Thông báo chủ trọ (không thông báo cho khách thuê)

                await notificationService.CreateAndSendNotificationAsync(
                    schedule.Rental.LandlordId,
                    "🔴 Khách thuê quá hạn thanh toán",
                    $"Phòng {schedule.Rental.Property?.Title}: Thanh toán tháng {schedule.PeriodMonth} " +
                    $"đã quá hạn {daysOverdue} ngày ({schedule.Amount:N0} VNĐ)",
                    "warning",
                    "/Landlord/Invoices"
                );
            }

            _logger.LogInformation($"Cảnh báo quá hạn: {overdueSchedules.Count} khoản");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi cảnh báo quá hạn: {ex.Message}");
        }
    }

    /// <summary>
    /// Nhắc hợp đồng sắp hết: Rental.EndDate trong 7 ngày tới + Status = "active"
    /// </summary>
    private async Task RemindExpiringContracts(
        NestFlowSystemContext context,
        INotificationService notificationService,
        DateOnly today)
    {
        try
        {
            var sevenDaysLater = today.AddDays(7);

            var expiringRentals = await context.Rentals
                .Include(r => r.Property)
                .Where(r => r.EndDate.HasValue 
                         && r.EndDate.Value >= today 
                         && r.EndDate.Value <= sevenDaysLater 
                         && r.Status == "active")
                .ToListAsync();

            foreach (var rental in expiringRentals)
            {
                var daysLeft = rental.EndDate!.Value.DayNumber - today.DayNumber;

                // Thông báo chủ trọ (không thông báo cho khách thuê)

                await notificationService.CreateAndSendNotificationAsync(
                    rental.LandlordId,
                    "📋 Hợp đồng sắp hết hạn",
                    $"Hợp đồng phòng {rental.Property?.Title} sẽ hết hạn trong {daysLeft} ngày " +
                    $"(ngày {rental.EndDate.Value:dd/MM/yyyy}). Hãy liên hệ người thuê để gia hạn.",
                    "info",
                    "/Landlord/Rentals"
                );
            }

            _logger.LogInformation($"Nhắc hợp đồng: {expiringRentals.Count} hợp đồng sắp hết hạn");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi nhắc hợp đồng: {ex.Message}");
        }
    }

    /// <summary>
    /// Nhắc hóa đơn đến hạn hoặc quá hạn cho Chủ trọ dựa trên bảng Invoices
    /// </summary>
    private async Task CheckInvoiceDueDates(
        NestFlowSystemContext context,
        INotificationService notificationService,
        DateOnly today)
    {
        try
        {
            var pendingInvoices = await context.Invoices
                .Include(i => i.Rental)
                    .ThenInclude(r => r.Property)
                .Where(i => i.Status == "pending" && i.DueDate.HasValue)
                .ToListAsync();

            int countDue = 0;
            int countOverdue = 0;

            foreach (var invoice in pendingInvoices)
            {
                if (invoice.Rental == null) continue;

                var landlordId = invoice.Rental.LandlordId;

                // Nếu hóa đơn đến hạn hôm nay
                if (invoice.DueDate!.Value == today)
                {
                    await notificationService.CreateAndSendNotificationAsync(
                        landlordId,
                        "Hóa đơn đến hạn",
                        $"Hóa đơn tháng {invoice.InvoiceMonth} của phòng {invoice.Rental.Property?.Title} " +
                        $"đã đến hạn thanh toán hôm nay ({invoice.TotalAmount:N0} VNĐ)",
                        "info",
                        "/Landlord/Invoices"
                    );
                    countDue++;
                }
                // Nếu hóa đơn đã quá hạn
                else if (invoice.DueDate!.Value < today)
                {
                    var daysOverdue = today.DayNumber - invoice.DueDate.Value.DayNumber;
                    await notificationService.CreateAndSendNotificationAsync(
                        landlordId,
                        "🔴 Hóa đơn quá hạn",
                        $"Hóa đơn tháng {invoice.InvoiceMonth} của phòng {invoice.Rental.Property?.Title} " +
                        $"đã quá hạn {daysOverdue} ngày ({invoice.TotalAmount:N0} VNĐ)",
                        "warning",
                        "/Landlord/Invoices"
                    );
                    countOverdue++;
                }
            }

            _logger.LogInformation($"Kiểm tra hóa đơn: {countDue} đến hạn, {countOverdue} quá hạn");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Lỗi nhắc hóa đơn: {ex.Message}");
        }
    }
}

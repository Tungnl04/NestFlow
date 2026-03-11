using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NestFlow.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public DashboardModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public int TotalRooms { get; set; }
        public int RentedRooms { get; set; }
        public int EmptyRooms { get; set; }
        public int ActiveListings { get; set; }

        public List<Listing> ExpiringListings { get; set; } = new List<Listing>();
        public List<ActivityItem> RecentActivities { get; set; } = new List<ActivityItem>();

        // Chart Data
        public Dictionary<string, int> PropertiesByType { get; set; } = new Dictionary<string, int>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Security check - assume only admin can access this route (usually handled by auth middleware, but we double check session here if needed)
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "admin")
            {
                return RedirectToPage("/Auth/Login");
            }

            // 1. Statistics
            TotalRooms = await _context.Properties.CountAsync(p => p.Status != "deleted");
            RentedRooms = await _context.Properties.CountAsync(p => p.Status == "rented");
            EmptyRooms = await _context.Properties.CountAsync(p => p.Status == "available");
            ActiveListings = await _context.Listings.CountAsync(l => l.Status == "published" || l.Status == "active");

            // 2. Expiring Listings (Expire within next 7 days, but not yet expired)
            var nextWeek = DateTime.UtcNow.AddDays(7);
            var now = DateTime.UtcNow;
            ExpiringListings = await _context.Listings
                .Include(l => l.Property)
                .Where(l => l.ExpiresAt > now && l.ExpiresAt <= nextWeek && (l.Status == "published" || l.Status == "active"))
                .OrderBy(l => l.ExpiresAt)
                .Take(5)
                .ToListAsync();

            // 3. Recent Activities (Merge newly registered users, new bookings, new listings)
            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(3)
                .Select(u => new ActivityItem
                {
                    Icon = "fas fa-user",
                    Color = "text-primary",
                    Message = $"Người dùng mới <strong>{u.FullName ?? u.Email}</strong> vừa đăng ký tài khoản.",
                    Time = u.CreatedAt ?? DateTime.UtcNow
                }).ToListAsync();

            var recentBookings = await _context.Bookings
                .Include(b => b.Property)
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .Select(b => new ActivityItem
                {
                    Icon = "fas fa-calendar-check",
                    Color = "text-success",
                    Message = $"Phòng <strong>{b.Property.Title}</strong> đã được gửi yêu cầu đặt lúc {b.BookingTime}.",
                    Time = b.CreatedAt ?? DateTime.UtcNow
                }).ToListAsync();

            var recentListings = await _context.Listings
                .Include(l => l.Property)
                .OrderByDescending(l => l.CreatedAt)
                .Take(3)
                .Select(l => new ActivityItem
                {
                    Icon = "fas fa-newspaper",
                    Color = "text-warning",
                    Message = $"Tin đăng mới <strong>{l.Title}</strong> vừa được tạo.",
                    Time = l.CreatedAt
                }).ToListAsync();

            // Merge, sort, and take top 5
            RecentActivities.AddRange(recentUsers);
            RecentActivities.AddRange(recentBookings);
            RecentActivities.AddRange(recentListings);
            RecentActivities = RecentActivities.OrderByDescending(a => a.Time).Take(5).ToList();

            // 4. Chart Data: Properties By Type
            var propertyStats = await _context.Properties
                .Where(p => p.Status != "deleted")
                .GroupBy(p => p.PropertyType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var stat in propertyStats)
            {
                var label = stat.Type switch
                {
                    "phong_tro" => "Phòng trọ",
                    "chung_cu" => "Chung cư mini",
                    "nha_nguyen_can" => "Nhà nguyên căn",
                    "studio" => "Phòng Studio",
                    "o_ghep" => "Ở ghép",
                    _ => "Khác"
                };
                PropertiesByType[label] = stat.Count;
            }

            return Page();
        }
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Time { get; set; }
    }
}

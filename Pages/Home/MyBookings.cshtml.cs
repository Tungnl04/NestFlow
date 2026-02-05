using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Home
{
    public class MyBookingsModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public MyBookingsModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public List<UserBookingViewModel> PendingBookings { get; set; } = new();
        public List<UserBookingViewModel> ConfirmedBookings { get; set; } = new();
        public List<UserBookingViewModel> RejectedBookings { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử booking";
                return RedirectToPage("/Home/Index");
            }

            // Check user type - Redirect landlord to their bookings page
            var user = await _context.Users.FindAsync((long)userId.Value);
            if (user != null && user.UserType?.ToLower() == "landlord")
            {
                return RedirectToPage("/Landlord/Bookings");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p.PropertyImages)
                .Include(b => b.Property.Landlord)
                .Where(b => b.RenterId == userId.Value)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            PendingBookings = bookings
                .Where(b => b.Status == "Pending")
                .Select(b => MapToViewModel(b))
                .ToList();

            ConfirmedBookings = bookings
                .Where(b => b.Status == "Confirmed")
                .Select(b => MapToViewModel(b))
                .ToList();

            RejectedBookings = bookings
                .Where(b => b.Status == "Rejected")
                .Select(b => MapToViewModel(b))
                .ToList();

            return Page();
        }

        private UserBookingViewModel MapToViewModel(Booking booking)
        {
            return new UserBookingViewModel
            {
                BookingId = booking.BookingId,
                PropertyId = booking.PropertyId,
                PropertyTitle = booking.Property.Title,
                PropertyImage = booking.Property.PropertyImages?.FirstOrDefault()?.ImageUrl ?? "/images/default-property.jpg",
                PropertyAddress = $"{booking.Property.Address}, {booking.Property.Ward}, {booking.Property.District}, {booking.Property.City}",
                DepositAmount = booking.Property.Deposit ?? 0,
                LandlordName = booking.Property.Landlord.FullName,
                LandlordPhone = booking.Property.Landlord.Phone,
                BookingDate = booking.BookingDate,
                BookingTime = booking.BookingTime,
                Status = booking.Status ?? "Pending",
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt ?? DateTime.Now
            };
        }
    }

    public class UserBookingViewModel
    {
        public long BookingId { get; set; }
        public long PropertyId { get; set; }
        public string PropertyTitle { get; set; } = null!;
        public string PropertyImage { get; set; } = null!;
        public string PropertyAddress { get; set; } = null!;
        public decimal DepositAmount { get; set; }
        public string LandlordName { get; set; } = null!;
        public string LandlordPhone { get; set; } = null!;
        public DateOnly BookingDate { get; set; }
        public TimeOnly BookingTime { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

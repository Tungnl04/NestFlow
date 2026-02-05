using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord
{
    public class BookingsModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public BookingsModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public List<BookingViewModel> PendingBookings { get; set; } = new();
        public List<BookingViewModel> ConfirmedBookings { get; set; } = new();
        public List<BookingViewModel> RejectedBookings { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null)
            {
                return RedirectToPage("/Home/Index");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p.PropertyImages)
                .Include(b => b.Renter)
                .Where(b => b.Property.LandlordId == landlordId.Value)
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

        private BookingViewModel MapToViewModel(Booking booking)
        {
            return new BookingViewModel
            {
                BookingId = booking.BookingId,
                PropertyId = booking.PropertyId,
                PropertyTitle = booking.Property.Title,
                PropertyImage = booking.Property.PropertyImages?.FirstOrDefault()?.ImageUrl ?? "/images/default-property.jpg",
                DepositAmount = booking.Property.Deposit ?? 0,
                RenterName = booking.Renter.FullName,
                RenterPhone = booking.Renter.Phone,
                RenterEmail = booking.Renter.Email,
                BookingDate = booking.BookingDate,
                BookingTime = booking.BookingTime,
                Status = booking.Status ?? "Pending",
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt ?? DateTime.Now
            };
        }
    }

    public class BookingViewModel
    {
        public long BookingId { get; set; }
        public long PropertyId { get; set; }
        public string PropertyTitle { get; set; } = null!;
        public string PropertyImage { get; set; } = null!;
        public decimal DepositAmount { get; set; }
        public string RenterName { get; set; } = null!;
        public string RenterPhone { get; set; } = null!;
        public string RenterEmail { get; set; } = null!;
        public DateOnly BookingDate { get; set; }
        public TimeOnly BookingTime { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

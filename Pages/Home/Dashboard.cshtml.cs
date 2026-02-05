using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Home
{
    public class DashboardModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public DashboardModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public int PendingBookingsCount { get; set; }
        public int ConfirmedBookingsCount { get; set; }
        public decimal WalletBalance { get; set; }
        public int SavedPropertiesCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Home/Index");
            }

            // Check user type - Redirect landlord to their dashboard
            var user = await _context.Users.FindAsync((long)userId.Value);
            if (user != null && user.UserType?.ToLower() == "landlord")
            {
                return RedirectToPage("/Landlord/Dashboard");
            }

            // Get booking counts
            PendingBookingsCount = await _context.Bookings
                .Where(b => b.RenterId == userId.Value && b.Status == "Pending")
                .CountAsync();

            ConfirmedBookingsCount = await _context.Bookings
                .Where(b => b.RenterId == userId.Value && b.Status == "Confirmed")
                .CountAsync();

            // Get or create wallet balance
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.LandlordId == userId.Value);

            if (wallet == null)
            {
                // Tạo ví mới nếu chưa có (trường hợp user cũ)
                wallet = new Wallet
                {
                    LandlordId = userId.Value,
                    AvailableBalance = 0,
                    LockedBalance = 0,
                    Currency = "VND",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            WalletBalance = wallet.AvailableBalance;

            // Get saved properties count
            SavedPropertiesCount = await _context.Favorites
                .Where(f => f.UserId == userId.Value)
                .CountAsync();

            return Page();
        }
    }
}

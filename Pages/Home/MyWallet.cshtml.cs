using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Home
{
    public class MyWalletModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public MyWalletModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public UserWalletInfo WalletInfo { get; set; } = new();
        public List<UserTransactionViewModel> Transactions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem ví";
                return RedirectToPage("/Home/Index");
            }

            // Check user type - Redirect landlord to their wallet page
            var user = await _context.Users.FindAsync((long)userId.Value);
            if (user != null && user.UserType?.ToLower() == "landlord")
            {
                return RedirectToPage("/Landlord/Wallet");
            }

            // Get or create wallet
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

            WalletInfo = new UserWalletInfo
            {
                WalletId = wallet.WalletId,
                AvailableBalance = wallet.AvailableBalance,
                Currency = wallet.Currency
            };

            // Get transactions
            Transactions = await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new UserTransactionViewModel
                {
                    WalletTxnId = t.WalletTxnId,
                    Direction = t.Direction,
                    Amount = t.Amount,
                    RelatedType = t.RelatedType,
                    Status = t.Status,
                    Note = t.Note,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Page();
        }
    }

    public class UserWalletInfo
    {
        public long WalletId { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "VND";
    }

    public class UserTransactionViewModel
    {
        public long WalletTxnId { get; set; }
        public string Direction { get; set; } = null!;
        public decimal Amount { get; set; }
        public string RelatedType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

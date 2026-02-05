using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Landlord
{
    public class WalletModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public WalletModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public WalletInfo WalletInfo { get; set; } = new();
        public List<TransactionViewModel> Transactions { get; set; } = new();
        public List<WithdrawViewModel> WithdrawRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null)
            {
                return RedirectToPage("/Home/Index");
            }

            // Get or create wallet
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.LandlordId == landlordId.Value);

            if (wallet == null)
            {
                // Tạo ví mới nếu chưa có
                wallet = new Wallet
                {
                    LandlordId = landlordId.Value,
                    AvailableBalance = 0,
                    LockedBalance = 0,
                    Currency = "VND",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            // Calculate pending withdrawals
            var pendingWithdrawals = await _context.WithdrawRequests
                .Where(w => w.LandlordId == landlordId.Value && w.Status == "Pending")
                .SumAsync(w => (decimal?)w.Amount) ?? 0;

            WalletInfo = new WalletInfo
            {
                WalletId = wallet.WalletId,
                AvailableBalance = wallet.AvailableBalance,
                LockedBalance = wallet.LockedBalance,
                PendingWithdrawals = pendingWithdrawals,
                TotalBalance = wallet.AvailableBalance + wallet.LockedBalance + pendingWithdrawals
            };

            // Get transactions
            Transactions = await _context.WalletTransactions
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new TransactionViewModel
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

            // Get withdraw requests
            WithdrawRequests = await _context.WithdrawRequests
                .Where(w => w.LandlordId == landlordId.Value)
                .OrderByDescending(w => w.RequestedAt)
                .Select(w => new WithdrawViewModel
                {
                    WithdrawId = w.WithdrawId,
                    Amount = w.Amount,
                    BankName = w.BankName,
                    BankAccount = w.BankAccount,
                    AccountHolder = w.AccountHolder,
                    Status = w.Status,
                    RequestedAt = w.RequestedAt,
                    ProcessedAt = w.ProcessedAt,
                    Note = w.Note
                })
                .ToListAsync();

            return Page();
        }
    }

    public class WalletInfo
    {
        public long WalletId { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal PendingWithdrawals { get; set; }
        public decimal TotalBalance { get; set; }
    }

    public class TransactionViewModel
    {
        public long WalletTxnId { get; set; }
        public string Direction { get; set; } = null!;
        public decimal Amount { get; set; }
        public string RelatedType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class WithdrawViewModel
    {
        public long WithdrawId { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; } = null!;
        public string BankAccount { get; set; } = null!;
        public string AccountHolder { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Note { get; set; }
    }
}

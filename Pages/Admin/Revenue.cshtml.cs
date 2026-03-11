using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;

namespace NestFlow.Pages.Admin
{
    public class RevenueModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public RevenueModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public decimal TotalRevenue { get; set; } = 0;
        public decimal SubscriptionRevenue { get; set; } = 0;
        public decimal CommissionRevenue { get; set; } = 0;
        public decimal TotalGMV { get; set; } = 0;

        // For Chart
        public Dictionary<string, decimal> MonthlyRevenue { get; set; } = new Dictionary<string, decimal>();

        // For Table
        public List<TransactionItem> RecentTransactions { get; set; } = new List<TransactionItem>();

        // New Advanced Metrics
        public Dictionary<string, decimal> RevenueByPlan { get; set; } = new Dictionary<string, decimal>();
        public List<TopLandlordItem> TopEarningLandlords { get; set; } = new List<TopLandlordItem>();
        public decimal MoMGrowthPercentage { get; set; }

        public class TopLandlordItem
        {
            public long LandlordId { get; set; }
            public string LandlordName { get; set; }
            public decimal TotalCommission { get; set; }
        }

        public class TransactionItem
        {
            public long PaymentId { get; set; }
            public string PayerName { get; set; }
            public string Type { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "admin")
            {
                return RedirectToPage("/Auth/Login");
            }

            var allCompletedPayments = await _context.Payments
                .Include(p => p.PayerUser)
                .Where(p => p.Status == "Completed")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            TotalGMV = allCompletedPayments.Sum(p => p.Amount);

            var subscriptions = allCompletedPayments.Where(p => p.PaymentType == "Subscription").ToList();
            var deposits = allCompletedPayments.Where(p => p.PaymentType == "Deposit").ToList();

            SubscriptionRevenue = subscriptions.Sum(p => p.Amount);
            CommissionRevenue = deposits.Sum(p => p.PlatformCommission ?? 0);
            
            TotalRevenue = SubscriptionRevenue + CommissionRevenue;

            // Generate Chart Data (Last 6 Months)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Today.AddMonths(-i))
                .OrderBy(d => d);

            foreach (var month in last6Months)
            {
                var subMonthKey = month.ToString("MM/yyyy");
                var sumSub = subscriptions
                    .Where(p => p.CreatedAt.Year == month.Year && p.CreatedAt.Month == month.Month)
                    .Sum(p => p.Amount);
                
                var sumCom = deposits
                    .Where(p => p.CreatedAt.Year == month.Year && p.CreatedAt.Month == month.Month)
                    .Sum(p => p.PlatformCommission ?? 0);
                
                MonthlyRevenue[subMonthKey] = sumSub + sumCom;
            }

            // MoM Growth
            var currentMonthRevenue = subscriptions.Where(p => p.CreatedAt.Year == DateTime.Today.Year && p.CreatedAt.Month == DateTime.Today.Month).Sum(p => p.Amount) +
                                      deposits.Where(p => p.CreatedAt.Year == DateTime.Today.Year && p.CreatedAt.Month == DateTime.Today.Month).Sum(p => p.PlatformCommission ?? 0);
            
            var lastMonth = DateTime.Today.AddMonths(-1);
            var lastMonthRevenue = subscriptions.Where(p => p.CreatedAt.Year == lastMonth.Year && p.CreatedAt.Month == lastMonth.Month).Sum(p => p.Amount) +
                                   deposits.Where(p => p.CreatedAt.Year == lastMonth.Year && p.CreatedAt.Month == lastMonth.Month).Sum(p => p.PlatformCommission ?? 0);

            if (lastMonthRevenue > 0)
            {
                MoMGrowthPercentage = ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
            }
            else
            {
                MoMGrowthPercentage = currentMonthRevenue > 0 ? 100 : 0;
            }

            // Revenue by Plan (Heuristic matching since SubscriptionId is often NULL)
            var subscriptionPayments = await _context.Payments
                .Where(p => p.PaymentType == "Subscription" && p.Status == "Completed")
                .ToListAsync();

            var allSubscriptions = await _context.LandlordSubscriptions
                .Include(s => s.Plan)
                .ToListAsync();

            RevenueByPlan = new Dictionary<string, decimal>();

            foreach (var payment in subscriptionPayments)
            {
                // Try to find matching subscription by LandlordId and time proximity (within 5 minutes)
                DateTime compareTime = payment.PaidAt ?? payment.CreatedAt;
                
                var matchedSub = allSubscriptions
                    .Where(s => s.LandlordId == payment.PayerUserId && 
                                Math.Abs((s.CreatedAt - compareTime).TotalMinutes) <= 5)
                    .OrderBy(s => Math.Abs((s.CreatedAt - compareTime).TotalMinutes))
                    .FirstOrDefault();

                string planName = matchedSub?.Plan?.PlanName ?? "Gói cơ bản/Khác";

                if (!RevenueByPlan.ContainsKey(planName))
                {
                    RevenueByPlan[planName] = 0;
                }
                RevenueByPlan[planName] += payment.Amount;
            }

            // Top Earning Landlords
            var commissionPayments = await _context.Payments
                .Include(p => p.Landlord)
                .Where(p => p.PaymentType == "Deposit" && p.Status == "Completed" && p.LandlordId != null)
                .ToListAsync();

            TopEarningLandlords = commissionPayments
                .GroupBy(p => p.LandlordId)
                .Select(g => new TopLandlordItem
                {
                     LandlordId = g.Key.Value,
                     LandlordName = g.First().Landlord?.FullName ?? "Landlord vô danh",
                     TotalCommission = g.Sum(p => p.PlatformCommission ?? 0)
                })
                .OrderByDescending(x => x.TotalCommission)
                .Take(5)
                .ToList();

            // Map Recent Transactions
            RecentTransactions = allCompletedPayments.Take(15).Select(p => new TransactionItem
            {
                PaymentId = p.PaymentId,
                PayerName = p.PayerUser?.FullName ?? "Khách hệ thống",
                Type = p.PaymentType == "Subscription" ? "Mua gói" : (p.PaymentType == "Deposit" ? "Đặt cọc (HH: " + (p.PlatformCommission?.ToString("N0") ?? "0") + ")" : p.PaymentType),
                Amount = p.Amount,
                Date = p.CreatedAt
            }).ToList();

            return Page();
        }
    }
}

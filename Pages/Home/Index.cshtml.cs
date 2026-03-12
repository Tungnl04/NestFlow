using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Models;
using NestFlow.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NestFlow.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly NestFlowSystemContext _context;

        public IndexModel(NestFlowSystemContext context)
        {
            _context = context;
        }

        public IList<PropertyViewModel> NewRooms { get; set; } = new List<PropertyViewModel>();
        public IList<PropertyViewModel> OneMonthRooms { get; set; } = new List<PropertyViewModel>();
        public IList<PropertyViewModel> ThreeMonthRooms { get; set; } = new List<PropertyViewModel>();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;
            
            // Fetch properties ordered by Priority and Date at database level
            var query = _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Reviews)
                .Include(p => p.Landlord)
                    .ThenInclude(u => u.LandlordSubscriptions)
                        .ThenInclude(s => s.Plan)
                .Where(p => p.Status == "available");

            // Define priority calculation consistent with Search page
            var sortedQuery = query
                .OrderByDescending(p => p.Landlord.LandlordSubscriptions
                    .Where(s => s.Status == "active" && s.EndAt > now)
                    .Max(s => (int?)s.Plan.PriorityLevel) ?? 0)
                .ThenByDescending(p => p.CreatedAt);

            var properties = await sortedQuery.ToListAsync();

            // Map to ViewModel
            var viewModels = properties.Select(p => {
                var activeSub = p.Landlord.LandlordSubscriptions
                    .Where(s => s.Status == "active" && s.EndAt > now)
                    .OrderByDescending(s => s.Plan.PriorityLevel)
                    .FirstOrDefault();

                return new PropertyViewModel
                {
                    Id = p.PropertyId,
                    Name = p.Title,
                    Price = p.Price ?? 0,
                    Location = $"{p.Ward}, {p.District}",
                    Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating ?? 0) : 0,
                    ReviewCount = p.Reviews.Count,
                    PostedDate = p.CreatedAt ?? now,
                    PriorityLevel = activeSub?.Plan.PriorityLevel ?? 0,
                    VipType = activeSub?.Plan.PlanName,
                    Image = p.PropertyImages.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl 
                            ?? "https://via.placeholder.com/300x200?text=No+Image"
                };
            }).ToList();

            var thirtyDaysAgo = now.AddDays(-30);
            var ninetyDaysAgo = now.AddDays(-90);

            // Categorize (already sorted by Priority and Date)
            NewRooms = viewModels
                .Where(v => v.PostedDate >= thirtyDaysAgo)
                .Take(8)
                .ToList();

            OneMonthRooms = viewModels
                .Where(v => v.PostedDate < thirtyDaysAgo && v.PostedDate >= ninetyDaysAgo)
                .Take(8)
                .ToList();

            ThreeMonthRooms = viewModels
                .Where(v => v.PostedDate < ninetyDaysAgo)
                .Take(8)
                .ToList();
        }
    }
}

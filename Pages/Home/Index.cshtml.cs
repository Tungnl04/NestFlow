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
            // Fetch all properties with images
            var properties = await _context.Properties
                .Include(p => p.PropertyImages)
                .Include(p => p.Reviews)
                .Where(p => p.Status == "available")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Map to ViewModel
            var viewModels = properties.Select(p => new PropertyViewModel
            {
                Id = p.PropertyId,
                Name = p.Title,
                Price = p.Price ?? 0,
                Location = $"{p.Ward}, {p.District}", 
                Rating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating ?? 0) : 0,
                ReviewCount = p.Reviews.Count,
                PostedDate = p.CreatedAt ?? DateTime.Now,
                IsFeatured = p.ViewCount > 100, 
                Image = p.PropertyImages.FirstOrDefault()?.ImageUrl ?? "https://via.placeholder.com/300x200?text=No+Image"
            }).ToList();

            // Split into categories 
            NewRooms = viewModels.Take(4).ToList();
            OneMonthRooms = viewModels.Skip(4).Take(4).ToList();
            ThreeMonthRooms = viewModels.Skip(8).Take(4).ToList();
        }
    }
}

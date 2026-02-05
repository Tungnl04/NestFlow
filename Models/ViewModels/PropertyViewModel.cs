using System;

namespace NestFlow.Models.ViewModels
{
    public class PropertyViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Location { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime PostedDate { get; set; }
        public bool IsFeatured { get; set; }
        public string Image { get; set; }
        public DateTime? CreatedAt { get; set; } // Added for Saved page (Date Saved)
    }
}

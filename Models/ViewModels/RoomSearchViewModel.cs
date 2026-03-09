using System.Collections.Generic;

namespace NestFlow.Models.ViewModels
{
    public class RoomSearchRequest
    {
        public string? Keyword { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinArea { get; set; }
        public decimal? MaxArea { get; set; }
        public List<string>? PropertyTypes { get; set; }
        public List<long>? Amenities { get; set; }
        public string? SortBy { get; set; } // price_asc, price_desc, new, area_desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class RoomSearchResponse
    {
        public IList<PropertyViewModel> Properties { get; set; } = new List<PropertyViewModel>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}

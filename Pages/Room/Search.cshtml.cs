using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using NestFlow.Models.ViewModels;

namespace NestFlow.Pages.Room
{
    public class SearchModel : PageModel
    {
        private readonly IPropertyService _propertyService;
        private readonly NestFlowSystemContext _context;

        public SearchModel(IPropertyService propertyService, NestFlowSystemContext context)
        {
            _propertyService = propertyService;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public RoomSearchRequest SearchRequest { get; set; } = new RoomSearchRequest();

        public RoomSearchResponse SearchResponse { get; set; } = new RoomSearchResponse();

        public List<Amenity> AllAmenities { get; set; } = new List<Amenity>();

        public async Task<IActionResult> OnGetAsync()
        {
            SearchResponse = await _propertyService.SearchRoomsAsync(SearchRequest);
            AllAmenities = await _context.Amenities.ToListAsync();
            return Page();
        }
    }
}

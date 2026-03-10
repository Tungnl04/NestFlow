using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;
using System.Security.Claims;

namespace NestFlow.Pages.Landlord.Posts
{
    public class CreateModel : PageModel
    {
        private readonly IListingService _listingService;
        private readonly IPropertyService _propertyService;
        private readonly IWebHostEnvironment _env;

        public CreateModel(IListingService listingService, IPropertyService propertyService, IWebHostEnvironment env)
        {
            _listingService = listingService;
            _propertyService = propertyService;
            _env = env;
        }

        [BindProperty]
        public Listing NewListing { get; set; } = new Listing();

        [BindProperty]
        public Property NewProperty { get; set; } = new Property();

        [BindProperty]
        public List<IFormFile> UploadedImages { get; set; } = new List<IFormFile>();

        [BindProperty]
        public List<IFormFile> Uploaded360Images { get; set; } = new List<IFormFile>();

        [BindProperty]
        public List<long> SelectedAmenityIds { get; set; } = new List<long>();

        public List<Amenity> AvailableAmenities { get; set; } = new List<Amenity>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userType = HttpContext.Session.GetString("UserType");
            if (userId == null || userType != "landlord") return RedirectToPage("/Home/Index");

            AvailableAmenities = await _propertyService.GetAllAmenitiesAsync();

            // Khởi tạo các giá trị mặc định nếu cần
            NewProperty.City = "TP. Hồ Chí Minh";
            NewProperty.Status = "available";
            NewProperty.PropertyType = "phong_tro";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Home/Index");

            // Re-load amenities for form in case of error
            AvailableAmenities = await _propertyService.GetAllAmenitiesAsync();

            // Bỏ qua xác thực cho các thuộc tính điều hướng và ID sẽ được gán tự động
            ModelState.Remove("NewListing.Property");
            ModelState.Remove("NewListing.Landlord");
            ModelState.Remove("NewListing.Status");
            ModelState.Remove("NewListing.PropertyId");
            ModelState.Remove("NewListing.LandlordId");
            
            ModelState.Remove("NewProperty.Landlord");
            ModelState.Remove("NewProperty.PropertyId");
            ModelState.Remove("NewProperty.LandlordId");
            ModelState.Remove("NewProperty.Status");
            ModelState.Remove("NewProperty.Title"); 
            ModelState.Remove("NewProperty.PropertyType");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // 1. Chuẩn bị dữ liệu Property & Listing
                NewProperty.LandlordId = userId.Value;
                NewListing.LandlordId = userId.Value; // Gán ID chủ trọ cho cả bài đăng
                NewProperty.Title = NewListing.Title;
                
                var imageEntities = new List<PropertyImage>();
                var propertyFolderGuid = Guid.NewGuid().ToString(); // Dùng GUID để folder là duy nhất trước khi có ID
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "properties", propertyFolderGuid);
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                int displayOrder = 0;

                // 2. Xử lý Ảnh thường
                if (UploadedImages != null && UploadedImages.Count > 0)
                {
                    foreach (var file in UploadedImages)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var newFileName = $"{Guid.NewGuid()}{extension}";
                        var filePath = Path.Combine(uploadsFolder, newFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imageEntities.Add(new PropertyImage
                        {
                            ImageUrl = $"/uploads/properties/{propertyFolderGuid}/{newFileName}",
                            IsPrimary = (displayOrder == 0),
                            DisplayOrder = displayOrder++
                        });
                    }
                }

                // 3. Xử lý Ảnh 360 độ
                if (Uploaded360Images != null && Uploaded360Images.Count > 0)
                {
                    foreach (var file in Uploaded360Images)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        // Hậu tố _360 rất quan trọng để hệ thống hiển thị đúng chế độ Panorama
                        var newFileName = $"{Guid.NewGuid()}_360{extension}";
                        var filePath = Path.Combine(uploadsFolder, newFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imageEntities.Add(new PropertyImage
                        {
                            ImageUrl = $"/uploads/properties/{propertyFolderGuid}/{newFileName}",
                            IsPrimary = (displayOrder == 0),
                            DisplayOrder = displayOrder++
                        });
                    }
                }

                // 4. Gọi Service thực hiện lưu DB trong 1 Transaction
                await _listingService.PublishNewListingWithPropertyAsync(NewListing, NewProperty, imageEntities, SelectedAmenityIds);

                TempData["Success"] = "Đăng tin thành công!";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi hệ thống: " + ex.Message);
                return Page();
            }
        }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace NestFlow.Models
{
    public class PropertyAmenity
    {
        [Column("property_id")]
        public long PropertyId { get; set; }
        [Column("amenity_id")]
        public long AmenityId { get; set; }

        // Navigation properties
        public virtual Property Property { get; set; } = null!;
        public virtual Amenity Amenity { get; set; } = null!;
    }
}

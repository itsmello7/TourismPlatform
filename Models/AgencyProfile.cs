using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismPlatform.Models
{
    public class AgencyProfile
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Agency name is required")]
        [StringLength(200, ErrorMessage = "Agency name cannot exceed 200 characters")]
        [Display(Name = "Agency Name")]
        public string AgencyName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }
        public ICollection<Tour>? Tours { get; set; }
    }
}

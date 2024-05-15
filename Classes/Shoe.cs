using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Shoe
    {
        public int Id { get; set; }
        [Required, Display(Name = "Brand")]
        public int BrandId { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        public List<string> Images { get; set; } = new List<string>();

        [Display(Name = "Favourites")]
        public ICollection<User> UsersList { get; set; } = new List<User>();
        [Display(Name = "Reviews")]
        public ICollection<ShoeReview> Reviews { get; set; } = new List<ShoeReview>();

        public Brand? Brand { get; set; }
    }
}

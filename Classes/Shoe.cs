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

        [Display(Name = "Comparison List")]
        public ICollection<Comparison> ComparisonList { get; set; } = new List<Comparison>();
        [Display(Name = "Favourites")]
        public ICollection<User> UsersList { get; set; } = new List<User>();

        public Brand? Brand { get; set; }
    }
}

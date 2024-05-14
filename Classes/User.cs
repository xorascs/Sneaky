using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class User
    {
        public int Id { get; set; }
        public Roles Role { get; set; }
        [Display(Name = "Favourites")]
        public int? FavouritesId { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string Password { get; set; }

        [Display(Name = "Comparison List")]
        public ICollection<Comparison> ComparisonList { get; set; } = new List<Comparison>();
        public ICollection<Shoe> Favourites { get; set; } = new List<Shoe>();

        public enum Roles
        {
            User,
            Admin
        }
    }
}

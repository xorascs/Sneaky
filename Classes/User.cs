using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class User
    {
        public int Id { get; set; }
        public Roles Role { get; set; }
        [Display(Name = "Comparison")]
        public int? ComparisonId {  get; set; }
        [Display(Name = "Favourites")]
        public int? FavouriteId { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string Password { get; set; }

        public Comparison? Comparison { get; set; }
        public Favourite? Favourite { get; set; }

        public enum Roles
        {
            User,
            Admin
        }
    }
}

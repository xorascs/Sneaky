using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Favourites
    {
        public int Id { get; set; }
        [Required, Display(Name = "User")]
        public int UserId { get; set; }
        public List<Shoe> Suggests { get; set; } = new List<Shoe>();

        public User? User { get; set; }
    }
}

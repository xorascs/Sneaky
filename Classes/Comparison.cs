using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Comparison
    {
        public int Id { get; set; }
        [Required, Display(Name = "User")]
        public int UserId { get; set; }

        [Display(Name = "Shoes List")]
        public ICollection<Shoe> ShoesList { get; set; } = new List<Shoe>();

        public User? User { get; set; }
    }
}

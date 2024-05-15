using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Favourite
    {
        public int Id { get; set; }

        public ICollection<Shoe> Shoes { get; set; } = new List<Shoe>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Brand
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
    }
}

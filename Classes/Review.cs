using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class Review
    {
        public int Id { get; set; }
        [Required, Display(Name = "User")]
        public int UserId { get; set; }
        [Required]
        public required string Comment { get; set; }
        [Required, Display(Name = "Created at")]
        public DateTime CreateCommentTime { get; set; }

        public User? User { get; set; }
    }
}

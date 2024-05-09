using System.ComponentModel.DataAnnotations;

namespace Sneaky.Classes
{
    public class User
    {
        public int Id { get; set; }
        public Roles Role { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string Password { get; set; }

        public enum Roles
        {
            User,
            Admin
        }
    }
}

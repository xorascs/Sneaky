using System.ComponentModel.DataAnnotations;

namespace Sneaky.Models
{
    public class UserFormViewModel
    {
        [Required]
        public required string Login;
        [Required]
        public required string Password;
    }
}

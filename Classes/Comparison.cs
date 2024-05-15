using Sneaky.Classes;
using System.ComponentModel.DataAnnotations;

public class Comparison
{
    public int Id { get; set; }

    public ICollection<Shoe> Shoes { get; set; } = new List<Shoe>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
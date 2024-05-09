using Microsoft.EntityFrameworkCore;
using Sneaky.Classes;

namespace Sneaky
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<Favourites> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Shoe> Shoes { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>().ToTable(nameof(Brand));
            modelBuilder.Entity<Comparison>().ToTable(nameof(Comparison));
            modelBuilder.Entity<Favourites>().ToTable(nameof(Favourites));
            modelBuilder.Entity<Review>().ToTable(nameof(Review));
            modelBuilder.Entity<Shoe>().ToTable(nameof(Shoe));
            modelBuilder.Entity<User>().ToTable(nameof(User));
        }
    }
}

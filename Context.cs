using Microsoft.EntityFrameworkCore;
using Sneaky.Classes;

namespace Sneaky
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) { }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Shoe> Shoes { get; set; }
        public DbSet<ShoeReview> ShoeReviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Favourite> Favorites { get; set; }
        public DbSet<Comparison> Comparisons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Brand>().ToTable(nameof(Brand));
            modelBuilder.Entity<Review>().ToTable(nameof(Review));
            modelBuilder.Entity<Shoe>().ToTable(nameof(Shoe));
            modelBuilder.Entity<ShoeReview>().ToTable(nameof(ShoeReview));
            modelBuilder.Entity<User>().ToTable(nameof(User));
            modelBuilder.Entity<Favourite>().ToTable(nameof(Favourite));
            modelBuilder.Entity<Comparison>().ToTable(nameof(Comparison));

            // Define the relationship between User and Favourite
            modelBuilder.Entity<User>()
                .HasOne(u => u.Favourite)
                .WithMany(f => f.Users)
                .HasForeignKey(u => u.FavouriteId);

            // Define the relationship between User and Comparison
            modelBuilder.Entity<User>()
                .HasOne(u => u.Comparison)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.ComparisonId);
        }
    }
}

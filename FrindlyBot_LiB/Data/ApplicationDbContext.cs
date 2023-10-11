using FrindlyBot_LiB.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FrindlyBot_LiB.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<BookModel> Books { get; set; }
        public DbSet<Reservations> reservations { get; set; }
        public DbSet<FrindlyBot_LiB.Models.Issued> Issued { get; set; } = default!;
    }
    
}
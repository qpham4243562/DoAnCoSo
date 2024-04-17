using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoAnCoSoAPI.Entities;

namespace DoAnCoSo.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<DoAnCoSoAPI.Entities.User_Post> User_Post { get; set; } = default!;
    }
}

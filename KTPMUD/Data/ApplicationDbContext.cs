
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using KTPMUD.Models;
using Microsoft.EntityFrameworkCore;


namespace KTPMUD.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BenhNhan> BenhNhans { get; set; }
        public DbSet<BacSi> BacSis { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<ThanhToanVienPhi> ThanhToanVienPhis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ThanhToanVienPhi>(entity =>
            {
                entity.Property(e => e.SoTien).HasPrecision(18, 2);
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Ofel.Infrastructure.Models;

namespace Ofel.Infrastructure.Data
{
    public class OfelDbContext : DbContext
    {
        public OfelDbContext(DbContextOptions<OfelDbContext> options) : base(options) { }

        public DbSet<SteelMaterialEntity> SteelMaterials { get; set; } = null!;
        public DbSet<SteelSectionEntity> SteelSections { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SteelMaterialEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired();
                b.Property(e => e.Standard).IsRequired();
            });

            modelBuilder.Entity<SteelSectionEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.ProfileType).IsRequired();
                b.Property(e => e.Name).IsRequired();
            });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WsApiexamen.Models;

namespace WsApiexamen.Data
{
    public class BdiExamenContext : DbContext
    {
        public BdiExamenContext(DbContextOptions<BdiExamenContext> options) : base(options) { }
        public DbSet<TblExamen> tblExamen => Set<TblExamen>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblExamen>(entidad =>
            {
                entidad.ToTable("tblExamen");
                entidad.HasKey(e => e.Id);
                
                entidad.Property(e => e.Id).ValueGeneratedNever();
                entidad.Property(e => e.Nombre).HasMaxLength(255).IsUnicode(false);
                entidad.Property(e => e.Descripcion).HasMaxLength(255).IsUnicode(false);
            });
        }

    }
}


using DevExpressSqlserver.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevExpressSqlserver.Infraestructure.Conexion
{
    public class SBERPDbContext : DbContext
    {
        public SBERPDbContext(DbContextOptions<SBERPDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<TipoGasto> TiposGasto { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<FondosMonetarios> FondosMonetarios { get; set; }
        public DbSet<DetalleMovimiento> DetalleMovimientos { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}

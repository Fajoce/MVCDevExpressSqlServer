
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
   
    }
}

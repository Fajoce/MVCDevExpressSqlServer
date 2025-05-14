using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.Entities
{
    public class Presupuesto
    {
        [Key]
        public int PresupuestoID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public int TipoGastoID { get; set; }
        public TipoGasto TipoGasto { get; set; }

        [Required]
        [StringLength(16)] // MM
        public string Mes { get; set; }

        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }
    }
}

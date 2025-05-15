using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.Entities
{
    public class Movimiento
    {
        [Key]
        public int MovimientoID { get; set; }

        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        public int FondoID { get; set; }
        public FondosMonetarios Fondo { get; set; }

        public string TipoMovimiento { get; set; } // Gasto / Deposito

        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }

        public string Observaciones { get; set; }

        public string NombreComercio { get; set; }

        public string TipoDocumento { get; set; } // Comprobante / Factura / Otro
    }
}

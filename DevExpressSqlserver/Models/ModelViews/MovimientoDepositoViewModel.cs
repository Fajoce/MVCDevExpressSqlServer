using DevExpressSqlserver.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.ModelViews
{
    public class MovimientoDepositoViewModel
    {
        public int DetalleID { get; set; }

        public int MovimientoID { get; set; }
        public Movimiento Movimiento { get; set; }

        public int TipoGastoID { get; set; }
        public string NombreGasto { get; set; }
        public TipoGasto TipoGasto { get; set; }

        [DataType(DataType.Currency)]
        public decimal Monto { get; set; }
        public int UsuarioId { get; set; }
    }
}

using DevExpressSqlserver.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.ModelViews
{
    public class PresupuestoModelView
    {
        public int PresupuestoID { get; set; }
        public int UsuarioID { get; set; }
        public Usuario Usuario { get; set; }

        public int TipoGastoID { get; set; }
        public TipoGasto TipoGasto { get; set; }
        public string Mes { get; set; }
        public decimal Monto { get; set; }
        public string NombreGasto { get; set; }
    }
}

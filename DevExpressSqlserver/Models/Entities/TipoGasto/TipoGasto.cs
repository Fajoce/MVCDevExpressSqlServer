using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.Entities
{
    public class TipoGasto
    {
        [Key]
        public int TipoGastoID { get; set; }

        [Required]
        public string Descripcion { get; set; }
        public int UsuarioId { get; set; }
        
    }
}

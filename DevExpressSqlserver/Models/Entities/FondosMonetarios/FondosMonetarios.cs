using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.Entities
{
    public class FondosMonetarios
    {
        [Key]
        public int FondoID { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [DataType(DataType.Currency)]
        public decimal Saldo { get; set; }
        public int UsuarioId { get; set; }
        
    }
}

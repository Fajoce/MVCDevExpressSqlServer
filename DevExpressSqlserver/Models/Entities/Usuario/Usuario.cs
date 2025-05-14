using System.ComponentModel.DataAnnotations;

namespace DevExpressSqlserver.Models.Entities
{
    public class Usuario
    {
        [Key]
        public int UsuarioID { get; set; }

        [Required]
        public string Identificacion { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        public string Direccion { get; set; }

        [EmailAddress]
        public string Correo { get; set; }

        public string Telefono { get; set; }
    }
}

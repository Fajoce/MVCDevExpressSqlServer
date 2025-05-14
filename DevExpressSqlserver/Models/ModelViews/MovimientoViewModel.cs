namespace DevExpressSqlserver.Models.ModelViews
{
    public class MovimientoViewModel
    {
        public int MovimientoID { get; set; }
        public int UsuarioID { get; set; }
        public DateTime Fecha { get; set; }
        public int FondoID { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Monto { get; set; }
        public string Observaciones { get; set; }
        public string NombreComercio { get; set; }
        public string TipoDocumento { get; set; }
        public string FondoNombre { get; set; }
        public string UsuarioNombre { get; set; }
    }
}

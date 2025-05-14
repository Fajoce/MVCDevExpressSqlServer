using DevExpressSqlserver.Models.Entities;

namespace DevExpressSqlserver.Models.Guards
{
    public static class TipoGastoGuard
    {
        public static void Validate(TipoGasto tipoGasto)
        {
            if (tipoGasto == null)
                throw new ArgumentNullException(nameof(tipoGasto), "El Tipo de gasto no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(tipoGasto.Descripcion))
                throw new ArgumentException("La descripcion es obligatorio.");

            if (tipoGasto.UsuarioId > 0)
                throw new ArgumentException("El Id  no puede ser 0");

        }
    }
}


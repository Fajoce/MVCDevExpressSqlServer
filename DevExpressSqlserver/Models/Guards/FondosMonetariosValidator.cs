using DevExpress.Xpo;
using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;

namespace DevExpressSqlserver.Models.Guards
{
    public  class FondosMonetariosValidator
    {
            public Task<(bool IsValid, string ErrorMessage)> ValidarAsync(FondosMonetarios fondo)
            {
                if (string.IsNullOrWhiteSpace(fondo.Descripcion))
                    return Task.FromResult((false, "La descripción no puede estar vacía."));

                if (fondo.Saldo < 0)
                    return Task.FromResult((false, "El saldo no puede ser negativo."));

                return Task.FromResult((true, string.Empty));
            }
        }
    }




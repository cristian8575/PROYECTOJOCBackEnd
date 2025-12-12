using System;
using System.ComponentModel.DataAnnotations;

namespace TEXTILJOC_ConcarWeb.Models
{
    // Clases placeholder para corregir errores de compilación
    // ya que no se encontraron en el workspace actual.

    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class SegUsuarioW
    {
        [Key]
        public required string CodUsuario { get; set; }
        public required string Nombre { get; set; }
    }

    public class SegEmpresasW
    {
        [Key]
        public required string CodEmpresa { get; set; }
        public required string RazonSocial { get; set; }
    }
}

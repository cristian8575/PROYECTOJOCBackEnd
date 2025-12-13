namespace TextilesJocApi.Models
{
    public class Usuario
    {
        public int Id { get; set; } // Será la Llave Primaria (PK)
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
    }
}

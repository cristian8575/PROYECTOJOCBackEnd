using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextilesJocApi.Models
{
    [Table("SEG_EMPRESAS_W")]
    public class SegEmpresasW
    {
        // 1. Cod_Empresa (CHAR 2) - Llave Primaria
        [Key]
        [Column(TypeName = "char(2)")]
        public string Cod_Empresa { get; set; } = string.Empty;

        // 2. Cod_Grup_Emp (CHAR 2)
        [Column(TypeName = "char(2)")]
        public string? Cod_Grup_Emp { get; set; }

        // 3. Des_Empresa (VARCHAR 60)
        [MaxLength(60)]
        public string? Des_Empresa { get; set; }

        // 4. Ruta_Logo (VARCHAR 60)
        [MaxLength(60)]
        public string? Ruta_Logo { get; set; }

        // 5. Num_Ruc (VARCHAR 11)
        [MaxLength(11)]
        public string? Num_Ruc { get; set; }

        // 6. Direccion (VARCHAR 100)
        [MaxLength(100)]
        public string? Direccion { get; set; }

        // 7. DSN (VARCHAR 200)
        [MaxLength(200)]
        public string? DSN { get; set; }

        // 8. Ruta0 (VARCHAR 60)
        [MaxLength(60)]
        public string? Ruta0 { get; set; }

        // 9. DSNSeguridad (VARCHAR 200)
        [MaxLength(200)]
        public string? DSNSeguridad { get; set; }

        // 10. Telefono (CHAR 20)
        [Column(TypeName = "char(20)")]
        public string? Telefono { get; set; }

        // 11. Fax (CHAR 20)
        [Column(TypeName = "char(20)")]
        public string? Fax { get; set; }

        // 12. Localidad (VARCHAR 50)
        [MaxLength(50)]
        public string? Localidad { get; set; }

        // 13. Lug_Cobranza (VARCHAR 100)
        [MaxLength(100)]
        public string? Lug_Cobranza { get; set; }

        // 14. Raz_Social (VARCHAR 200)
        [MaxLength(200)]
        public string? Raz_Social { get; set; }

        // 15. cod_razsocial (CHAR 4)
        [Column(TypeName = "char(4)")]
        public string? cod_razsocial { get; set; }

        // 16. Contacto (VARCHAR 100)
        [MaxLength(100)]
        public string? Contacto { get; set; }

        // 17. Des_Comp_Emp (VARCHAR 100)
        [MaxLength(100)]
        public string? Des_Comp_Emp { get; set; }

        // 18. Direccion_Letras (VARCHAR 100)
        [MaxLength(100)]
        public string? Direccion_Letras { get; set; }

        // 19. Telefono_Letras (CHAR 30)
        [Column(TypeName = "char(30)")]
        public string? Telefono_Letras { get; set; }

        // 20. DSN_NET (VARCHAR 100)
        [MaxLength(100)]
        public string? DSN_NET { get; set; }

        // 21. DSNSeguridad_NET (VARCHAR 100)
        // Nota: En la imagen se corta el nombre, asumo que es "_NET" por el contexto de la fila 20
        [MaxLength(100)]
        public string? DSNSeguridad_NET { get; set; }

        // 22. Ruta_Icono (VARCHAR 100)
        [MaxLength(100)]
        public string? Ruta_Icono { get; set; }

        // --- COLORES (SMALLINT se convierte a short) ---

        // 23. ColorFondo_R (SMALLINT)
        public short? ColorFondo_R { get; set; }

        // 24. ColorFondo_G (SMALLINT)
        public short? ColorFondo_G { get; set; }

        // 25. ColorFondo_B (SMALLINT)
        public short? ColorFondo_B { get; set; }

        // -----------------------------------------------

        // 26. DSN_VPN (VARCHAR 250)
        [MaxLength(250)]
        public string? DSN_VPN { get; set; }

        // 27. DSN_SEGURIDAD_VPN (VARCHAR 250)
        // Nota: En la imagen se corta como "DSN_SEGURI...", asumo que termina en VPN
        [MaxLength(250)]
        public string? DSN_SEGURIDAD_VPN { get; set; }

        // 28. DataBaseName (VARCHAR 100)
        [MaxLength(100)]
        public string? DataBaseName { get; set; }

        // 29. ServerName (VARCHAR 100)
        [MaxLength(100)]
        public string? ServerName { get; set; }

        // 30. ServerIPName (VARCHAR 100)
        [MaxLength(100)]
        public string? ServerIPName { get; set; }

        // 31. TipDataBase (VARCHAR 10)
        [MaxLength(10)]
        public string? TipDataBase { get; set; }
    }
}
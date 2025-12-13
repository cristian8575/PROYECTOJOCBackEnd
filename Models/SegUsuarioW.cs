using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextilesJocApi.Models
{
    [Table("SEG_USUARIOS_W")]
    public class SegUsuarioW
    {
        // 1. Cod_Usuario
        [Column(TypeName = "char(15)")]
        public string Cod_Usuario { get; set; } = string.Empty;

        // 2. Nom_Usuario
        [MaxLength(300)]
        public string? Nom_Usuario { get; set; }

        // 3. Password
        [MaxLength(15)]
        public string? Password { get; set; }

        // 4. Nom_Foto
        [MaxLength(60)]
        public string? Nom_Foto { get; set; }

        // 5. Observacion
        [MaxLength(60)]
        public string? Observacion { get; set; }

        // 6. AccionOc
        [Column(TypeName = "char(1)")]
        public string? AccionOc { get; set; }

        // 7. Firma
        [MaxLength(60)]
        public string? Firma { get; set; }

        // 8. newpass
        [Column(TypeName = "char(10)")]
        public string? Newpass { get; set; }

        // 9. Nro_Reemplazo
        public int? Nro_Reemplazo { get; set; }

        // 10. Flg_Control_Documentario
        [Column(TypeName = "char(1)")]
        public string? Flg_Control_Documentario { get; set; }

        // 11. Flg_Verifica_Estacion
        [Column(TypeName = "char(1)")]
        public string? Flg_Verifica_Estacion { get; set; }

        // 12. Flg_FavoritosDefecto
        [Column(TypeName = "char(1)")]
        public string? Flg_FavoritosDefecto { get; set; }

        // 13. Flg_SetFocus_Defecto
        [Column(TypeName = "char(1)")]
        public string? Flg_SetFocus_Defecto { get; set; }

        // 14. Flg_AutoHide_Defecto
        [Column(TypeName = "char(1)")]
        public string? Flg_AutoHide_Defecto { get; set; }

        // 15. Flg_Activo
        [Column(TypeName = "char(1)")]
        public string? Flg_Activo { get; set; }

        // 16. Fec_Baja (Fecha)
        public DateTime? Fec_Baja { get; set; }

        // 17. Email
        [MaxLength(100)]
        public string? Email { get; set; }

        // 18. flg_personal_costos_sistemas
        [Column(TypeName = "char(1)")]
        public string? flg_personal_costos_sistemas { get; set; }

        // 19. Id_Usuario (Llave Primaria al final, tal cual la imagen)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Autoincremental
        public int Id_Usuario { get; set; }


    }
}
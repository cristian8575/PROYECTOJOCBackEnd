namespace TEXTILJOC_ConcarWeb.Models
{
    public class MantenimientoCuentasBancosRequest
    {
        public string? Accion { get; set; }
        public int? ID { get; set; }
        public string? Cod_Cuenta { get; set; }
        public string? Descripcion { get; set; }
        public string? Nombre_Banco { get; set; }
        public string? Numero_Cuenta { get; set; }
        public string? Id_Moneda { get; set; }
        public string? Mod_EstadoCuenta { get; set; }
        public string? Ent_Financiera { get; set; }
        public string? Tipo_Cuenta_Reporte { get; set; }
        public string? Orden_Reporte { get; set; }
        public string? Cuenta_Contable { get; set; }
    }
}

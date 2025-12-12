namespace TEXTILJOC_ConcarWeb.Models
{
    public class AnexoRequest
    {
        public string? Cod_TipAnex { get; set; }
        public string? Cod_Anxo { get; set; }
        public string? Des_Anexo { get; set; }
        public string? Num_Ruc { get; set; }
        public string? Dir_Anexo { get; set; }
        public string? Cod_TipoPersona { get; set; }
        public string? Apellido_Paterno { get; set; }
        public string? Apellido_Materno { get; set; }
        public string? Nombres { get; set; }
        public string? Cod_TipoDoc { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Cod_Ubigeo { get; set; }
        public string? EntidadFinanciera { get; set; }
        public string? Cod_Sexo { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? EssaludVida { get; set; }
        public string? TipoComision { get; set; }
        public string? Cuspp { get; set; }
        public string? NombreVia { get; set; }
        public string? NumeroAnexo { get; set; }
        public string? Interior { get; set; }
        public string? Cod_Zona { get; set; }
        public string? NombreZona { get; set; }
        public string? ReferenciaZona { get; set; }

        // Bools as string "1"/"0"
        public string? Es_Nacional { get; set; }
        public string? Es_Extranjero { get; set; }
        public string? Es_Vigente { get; set; }
        public string? Es_Anulado { get; set; }
        public string? Es_Domiciliado { get; set; }
        public string? AplicaConvenio { get; set; }
        public string? FormSuspension { get; set; }

        // Mapped properties
        public string? Cod_Situacion { get; set; }
        public string? Cod_Nacionalidad { get; set; }
        public string? Cod_TipoVia { get; set; }
        public string? Cod_Moneda { get; set; }
        public string? Flag_Essalud { get; set; }
        public string? Cod_TipoComision { get; set; }
        public string? Formulario_Susp { get; set; }
        public string? Tasa_Detraccion { get; set; }
        public string? Tasa_Percepcion { get; set; }

        // Additional safe defaults
        public string? Ruc_Ant { get; set; }
        public string? Flg_RecHon { get; set; }
        public string? Cod_TipIte { get; set; }
        public string? flg_proveedor_especial { get; set; }
        public string? Aval_Cod_TipAnEX { get; set; }
        public string? Aval_Cod_Anxo { get; set; }
        public string? Sec_CtaCte_Default { get; set; }
        public string? Flg_Buen_Contribuyente { get; set; }
        public string? Resolucion_Buen_Contribuyente { get; set; }
        public string? Flg_Agente_Retencion_IGV { get; set; }
        public string? Resolucion_Agente_Retencion_IGV { get; set; }
        public string? COD_CLIENTE { get; set; }
        public string? Flg_Considerado_Para_Retencion { get; set; }
        public string? Cod_Banco_Default_Soles { get; set; }
        public string? Sec_CtaCte_Default_Soles { get; set; }
        public string? Cod_Banco_Default_Otros { get; set; }
        public string? Sec_CtaCte_Default_Otros { get; set; }
        public string? Cod_CtaCte_Detracciones { get; set; }
        public string? Servicios_Clase { get; set; }
        public string? Fec_Alta { get; set; }
        public string? Flg_Agente_Percepcion { get; set; }
        public string? Fec_Inicio_Agente_Percepcion { get; set; }
        public string? Resolucion_Agente_Percepcion { get; set; }
        public string? Nro_DocIde { get; set; }
        public string? Lead_Time_Pago_Cliente { get; set; }
        public string? Cod_Tip_proveedor_area_negocio { get; set; }
        public string? Flg_Pago_Restringido { get; set; }
        public string? Fec_Pago_Restringido { get; set; }
        public string? Cod_Usuario_Pago_Restringido { get; set; }
        public string? Obs_Pago_Restringido { get; set; }
        public string? Fec_Baja { get; set; }
        public string? Cod_Usuario_Baja { get; set; }
        public string? Cod_Motivo_Baja { get; set; }
        public string? Observaciones_Baja { get; set; }
        public string? Cod_Tipo_Doc_Proveedor { get; set; }
        public string? Flg_Pagar_Detraccion { get; set; }
        public string? Cod_Trabajador { get; set; }
        public string? Tip_Trabajador { get; set; }
        public string? Limite_Dolares { get; set; }
        public string? Limite_Soles { get; set; }
        public string? Flg_Cliente_Especial { get; set; }
        public string? Fec_Bloqueo { get; set; }
        public string? Cod_usuario { get; set; }
        public string? Pc_Bloqueo { get; set; }
        public string? Flg_BloqueoDespacho { get; set; }
        public string? Dir_FACELE { get; set; }
        public string? ID_CLIE_DEUDOR { get; set; }
        public string? Nro_Doc_Ide_No_Domiciliados { get; set; }
        public string? Fec_Creacion_1 { get; set; }
    }
}

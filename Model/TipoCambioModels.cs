using System;

namespace TEXTILJOC_ConcarWeb.Models
{
    public class MonedaDto
    {
        public required string Clave { get; set; }
        public required string Descripcion { get; set; }
    }

    public class TipoCambioDto
    {
        public required string Dia { get; set; }
        public decimal TipoCambioCompra { get; set; }
        public decimal TipoCambioVenta { get; set; }
        public required string FecCreacion { get; set; }
    }

    public class TipoCambioRequest
    {
        public required string Moneda { get; set; }
        public required string Anio { get; set; }
        public required string Mes { get; set; }
        public required string Dia { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
    }

    public class TipoCambioMasivoRequest
    {
        public required string Moneda { get; set; }
        public required string Anio { get; set; }
        public required string Mes { get; set; }
        public required string DiaDesde { get; set; }
        public required string DiaHasta { get; set; }
        public decimal Compra { get; set; }
        public decimal Venta { get; set; }
    }
}
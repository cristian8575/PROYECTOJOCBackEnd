using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.Data;
using TEXTILJOC_ConcarWeb.Data;
using TEXTILJOC_ConcarWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Net.Http;

namespace TEXTILJOC_ConcarWeb.Controllers
{
    [Route("api/tipocambio/reporte")]
    [ApiController]
    public class ReporteTipoCambioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        // BCRP Series
        // USD Interbancario
        private const string SeriesUsdCompra = "PD04639PD";
        private const string SeriesUsdVenta = "PD04640PD";

        // EUR Bancario (Promedio) - Best guess for standard accounting use
        private const string SeriesEurCompra = "PD04647PD";
        private const string SeriesEurVenta = "PD04648PD";

        public ReporteTipoCambioController(ApplicationDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpPost("excel")]
        public async Task<IActionResult> GenerateExcel([FromBody] ReporteTipoCambioRequest req)
        {
            try
            {
                // Validate and Trim inputs
                req.Moneda = req.Moneda?.Trim();
                req.Anio = req.Anio?.Trim();
                req.Mes = req.Mes?.Trim();

                var dt = new DataTable();
                dt.Columns.Add("Dia", typeof(string));
                dt.Columns.Add("TipoCambioCompra", typeof(decimal));
                dt.Columns.Add("TipoCambioVenta", typeof(decimal));

                bool sourcedFromBcrp = false;

                // 1. Reuse logic from TipoCambioController directly
                try
                {
                    var controller = new TipoCambioController(_context);
                    var actionResult = await controller.GetList(req.Moneda, req.Anio, req.Mes);

                    if (actionResult is OkObjectResult okResult && okResult.Value is List<TipoCambioDto> list)
                    {
                        foreach (var item in list)
                        {
                            var row = dt.NewRow();
                            row["Dia"] = item.Dia;
                            row["TipoCambioCompra"] = item.TipoCambioCompra;
                            row["TipoCambioVenta"] = item.TipoCambioVenta;
                            dt.Rows.Add(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error invoking TipoCambioController: " + ex.Message);
                }

                // 2. Fallbacks if empty
                if (dt.Rows.Count == 0)
                {
                    if (req.Moneda == "MN" || req.Moneda == "PEN")
                    {
                        // Generate dummy data for Soles (1.000)
                        GenerateSolesData(dt, req.Anio, req.Mes);
                        sourcedFromBcrp = false; // Not BCRP, just generated
                    }
                    else
                    {
                        // Try BCRP for USD or EUR
                        var dtBcrp = await FetchFromBcrp(req.Moneda, req.Anio, req.Mes);
                        if (dtBcrp.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtBcrp.Rows)
                            {
                                var newRow = dt.NewRow();
                                newRow["Dia"] = r["Dia"];
                                newRow["TipoCambioCompra"] = r["TipoCambioCompra"];
                                newRow["TipoCambioVenta"] = r["TipoCambioVenta"];
                                dt.Rows.Add(newRow);
                            }
                            sourcedFromBcrp = true;
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { message = $"No se encontraron registros de Tipo de Cambio para {req.Moneda} en el periodo seleccionado." });
                }

                // Month Name
                string nombreMes = "";
                if (int.TryParse(req.Mes, out int mesNum) && mesNum >= 1 && mesNum <= 12)
                {
                    nombreMes = new DateTime(2000, mesNum, 1).ToString("MMMM", new CultureInfo("es-PE")).ToUpper();
                }
                else
                {
                    nombreMes = req.Mes ?? "";
                }

                string monedaDesc = req.Moneda == "USD" ? "DOLAR USA" : req.Moneda == "EUR" ? "EURO" : req.Moneda == "MN" ? "NACIONAL" : req.Moneda;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("TipoCambio");

                    // Report Header
                    worksheet.Cell(1, 1).Value = "TEXTILESJOC S A C";
                    worksheet.Cell(1, 10).Value = "PAG.    1";

                    worksheet.Cell(2, 1).Value = "CTCAMB03";
                    worksheet.Cell(2, 10).Value = DateTime.Now.ToString("dd/MM/yyyy");

                    var sourceText = sourcedFromBcrp ? " (FUENTE: BCRP)" : "";
                    worksheet.Cell(4, 3).Value = $"TIPO CAMBIO : {req.Moneda}       {monedaDesc} DEL MES DE {nombreMes} DE {req.Anio}{sourceText}";
                    worksheet.Range(4, 3, 4, 8).Merge().Style.Font.Bold = true;

                    // Table Headers
                    int row = 6;
                    worksheet.Cell(row, 2).Value = "DIA";
                    worksheet.Cell(row, 4).Value = "CAMBIO COMPRA (M)";
                    worksheet.Cell(row, 6).Value = "CAMBIO VENTA (V)";

                    worksheet.Cell(row + 1, 2).Value = "---";
                    worksheet.Cell(row + 1, 4).Value = "-----------------";
                    worksheet.Cell(row + 1, 6).Value = "----------------";

                    row += 2;

                    foreach (DataRow dr in dt.Rows)
                    {
                        string diaFull = dr["Dia"]?.ToString() ?? "";
                        string dia = diaFull;

                        if (diaFull.Contains("/"))
                        {
                            var parts = diaFull.Split('/');
                            if (parts.Length > 0) dia = parts[0];
                        }

                        if (dia.Length == 1) dia = "0" + dia;

                        decimal compra = Convert.ToDecimal(dr["TipoCambioCompra"] ?? 0);
                        decimal venta = Convert.ToDecimal(dr["TipoCambioVenta"] ?? 0);

                        worksheet.Cell(row, 2).Value = dia;
                        worksheet.Cell(row, 4).Value = compra;
                        worksheet.Cell(row, 6).Value = venta;

                        worksheet.Cell(row, 4).Style.NumberFormat.Format = "0.000";
                        worksheet.Cell(row, 6).Style.NumberFormat.Format = "0.000";

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TipoCambio_{req.Anio}_{req.Mes}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generando Excel: " + ex.Message + " | Stack: " + ex.StackTrace });
            }
        }

        private void GenerateSolesData(DataTable dt, string? anio, string? mes)
        {
            if (string.IsNullOrEmpty(anio) || string.IsNullOrEmpty(mes)) return;
            try
            {
                int y = int.Parse(anio);
                int m = int.Parse(mes);
                int days = DateTime.DaysInMonth(y, m);
                for (int i = 1; i <= days; i++)
                {
                    var row = dt.NewRow();
                    row["Dia"] = i.ToString("00");
                    row["TipoCambioCompra"] = 1.000m;
                    row["TipoCambioVenta"] = 1.000m;
                    dt.Rows.Add(row);
                }
            }
            catch { }
        }

        private async Task<DataTable> FetchFromBcrp(string moneda, string? anio, string? mes)
        {
            var dt = new DataTable();
            dt.Columns.Add("Dia", typeof(string));
            dt.Columns.Add("TipoCambioCompra", typeof(decimal));
            dt.Columns.Add("TipoCambioVenta", typeof(decimal));

            if (string.IsNullOrEmpty(anio) || string.IsNullOrEmpty(mes)) return dt;

            string sCompra = "", sVenta = "";
            if (moneda == "USD") { sCompra = SeriesUsdCompra; sVenta = SeriesUsdVenta; }
            else if (moneda == "EUR") { sCompra = SeriesEurCompra; sVenta = SeriesEurVenta; }
            else return dt; // No series for other currencies

            int y = int.Parse(anio);
            int m = int.Parse(mes);
            var startDate = new DateTime(y, m, 1).ToString("yyyy-M-d");
            var endDate = new DateTime(y, m, DateTime.DaysInMonth(y, m)).ToString("yyyy-M-d");

            string url = $"https://estadisticas.bcrp.gob.pe/estadisticas/series/api/{sCompra}-{sVenta}/json/{startDate}/{endDate}";

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using (var doc = JsonDocument.Parse(response))
                {
                    var periods = doc.RootElement.GetProperty("periods");
                    foreach (var period in periods.EnumerateArray())
                    {
                        var name = period.GetProperty("name").GetString();
                        var values = period.GetProperty("values");

                        var day = name?.Split('.')[0];
                        if (values.GetArrayLength() >= 2)
                        {
                            var row = dt.NewRow();
                            row["Dia"] = day;

                            if (decimal.TryParse(values[0].GetString(), out decimal c)) row["TipoCambioCompra"] = c;
                            if (decimal.TryParse(values[1].GetString(), out decimal v)) row["TipoCambioVenta"] = v;

                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("BCRP Fetch Error: " + ex.Message);
            }

            return dt;
        }
    }

    public class ReporteTipoCambioRequest
    {
        public string? Moneda { get; set; }
        public string? Anio { get; set; }
        public string? Mes { get; set; }
    }
}

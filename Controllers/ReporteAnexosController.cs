using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel;
using System.Data;
using TEXTILJOC_ConcarWeb.Data;
using TEXTILJOC_ConcarWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace TEXTILJOC_ConcarWeb.Controllers
{
    [Route("api/anexos/reporte")]
    [ApiController]
    public class ReporteAnexosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReporteAnexosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("excel")]
        public async Task<IActionResult> GenerateExcel([FromBody] ReporteAnexoRequest req)
        {
            try
            {
                var dt = new DataTable();
                // Ensure correct connection access
                var connection = _context.Database.GetDbConnection();
                if (connection == null) return StatusCode(500, "Error de conexión BD");

                // Use the connection directly
                // Note: GetDbConnection returns usually the connection associated with EF.
                // We should open it if closed.
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCAR_EMISION_ANEXOS";
                    command.CommandType = CommandType.StoredProcedure;

                    // Parameters
                    var pAccion = command.CreateParameter();
                    pAccion.ParameterName = "@Accion";
                    pAccion.Value = "CONSULTAR_DETALLE";
                    command.Parameters.Add(pAccion);

                    var pCodigo = command.CreateParameter();
                    pCodigo.ParameterName = "@CodigoAnexo";
                    pCodigo.Value = (object?)req.Cod_TipAnEX ?? DBNull.Value;
                    command.Parameters.Add(pCodigo);

                    var pOrden = command.CreateParameter();
                    pOrden.ParameterName = "@Orden";
                    pOrden.Value = (object?)req.Orden ?? "CODIGO";
                    command.Parameters.Add(pOrden);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }

                // Do not close connection here if it's managed by DI/EF scope, but opening it manually is safe if we check state.
                // Actually EF Core connection is scoped. We leave it as is.

                if (dt.Rows.Count == 0)
                {
                    return NotFound(new { message = "No se encontraron registros para generar el reporte." });
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Anexos");

                    // Headers
                    worksheet.Cell(1, 1).Value = "REPORTE DE RELACIÓN DE ANEXOS";
                    worksheet.Range(1, 1, 1, 6).Merge().Style.Font.Bold = true;
                    worksheet.Range(1, 1, 1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int row = 3;
                    worksheet.Cell(row, 1).Value = "Tipo";
                    worksheet.Cell(row, 2).Value = "Código";
                    worksheet.Cell(row, 3).Value = "Descripción / Razón Social";
                    worksheet.Cell(row, 4).Value = "RUC";
                    worksheet.Cell(row, 5).Value = "Dirección";
                    worksheet.Cell(row, 6).Value = "Teléfono";

                    var headerRange = worksheet.Range(row, 1, row, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                    row++;
                    foreach (DataRow dr in dt.Rows)
                    {
                        worksheet.Cell(row, 1).Value = dr["Cod_TipAnEX"]?.ToString();
                        worksheet.Cell(row, 2).Value = dr["Cod_Anxo"]?.ToString();
                        worksheet.Cell(row, 3).Value = dr["Des_Anexo"]?.ToString();
                        worksheet.Cell(row, 4).Value = dr["Num_Ruc"]?.ToString();
                        worksheet.Cell(row, 5).Value = dr["Dir_Anexo"]?.ToString();
                        worksheet.Cell(row, 6).Value = dr["Telefono"]?.ToString();
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Relacion_Anexos.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generando Excel: " + ex.Message });
            }
        }
    }

    public class ReporteAnexoRequest
    {
        public string? Cod_TipAnEX { get; set; }
        public string? Orden { get; set; } // "CODIGO" or "NOMBRE"
    }
}

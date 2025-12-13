using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using ClosedXML.Excel;

namespace TextilesJocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SqlController : ControllerBase
    {

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public SqlController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env; // Permite acceder a la ruta de la carpeta de la web
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_config.GetConnectionString("CadenaSQL"));
        }

        // Endpoint SIMPLE (ideal login, retorna solo 1 value)
        [HttpPost("procedure/single")]
        public async Task<IActionResult> ExecuteProcedureSingle([FromBody] ProcedureRequest req)
        {
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(req.Nombre, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                if (req.Parametros != null)
                {
                    foreach (var kv in req.Parametros)
                    {
                        cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value ?? "");
                    }
                }

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    string result = reader.GetValue(0)?.ToString() ?? "";
                    return Ok(new { value = result });
                }

                return Ok(new { value = "" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Endpoint GRID (devuelve lista de objetos -> usable para tablas)
        [HttpPost("procedure/grid")]
        public async Task<IActionResult> ExecuteProcedureGrid([FromBody] ProcedureRequest req)
        {
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(req.Nombre, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0; // Aumenta el tiempo de espera a 120 segundos

                if (req.Parametros != null)
                {
                    foreach (var kv in req.Parametros)
                    {
                        cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value ?? "");
                    }
                }

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                var list = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string nombreColumna = reader.GetName(i).ToLower();

                        row[nombreColumna] = reader.GetValue(i);
                        //row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    list.Add(row);
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Endpoint EXCEL (devuelve lista de objetos -> usable para tablas)

        [HttpPost("generate-excel-report")]
        public async Task<IActionResult> GenerateExcelReport([FromBody] ProcedureRequest req)
        {
            try
            {
                // 1. Obtener los datos del procedimiento almacenado
                var dataTable = new DataTable();
                using (var conn = GetConnection())
                {
                    using (var cmd = new SqlCommand(req.Nombre, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;  

                     
                        DateTime fechaIni = DateTime.ParseExact(req.Parametros["FECHA"], "dd/MM/yyyy", null);
                        DateTime fechaFin = DateTime.ParseExact(req.Parametros["FECHA1"], "dd/MM/yyyy", null);

                        cmd.Parameters.AddWithValue("@FECHA", fechaIni);
                        cmd.Parameters.AddWithValue("@FECHA1", fechaFin);
                        cmd.Parameters.AddWithValue("@FLG_REPROCESO", req.Parametros["FLG_REPROCESO"]);
                        cmd.Parameters.AddWithValue("@OPCION", req.Parametros["OPCION"]);
                        cmd.Parameters.AddWithValue("@Cod_GamCol", req.Parametros["Cod_GamCol"]);
                        cmd.Parameters.AddWithValue("@Cod_TipoReceta", req.Parametros["Cod_TipoReceta"]);
                        cmd.Parameters.AddWithValue("@Cod_IntCol", req.Parametros["Cod_IntCol"]);
                        cmd.Parameters.AddWithValue("@Cod_Ordtra", req.Parametros["Cod_Ordtra"]);
                        cmd.Parameters.AddWithValue("@Cod_Cliente_Tex", req.Parametros["Cod_Cliente_Tex"]);
                        cmd.Parameters.AddWithValue("@COD_USUARIO", req.Parametros["COD_USUARIO"]);
                        cmd.Parameters.AddWithValue("@str_GastoOtrosDOL", req.Parametros["str_GastoOtrosDOL"]);
                        cmd.Parameters.AddWithValue("@str_GastoLuzSOL", req.Parametros["str_GastoLuzSOL"]);
                        cmd.Parameters.AddWithValue("@str_GastoAguaSOL", req.Parametros["str_GastoAguaSOL"]);
                        cmd.Parameters.AddWithValue("@str_GastoGasSOL", req.Parametros["str_GastoGasSOL"]);
                        cmd.Parameters.AddWithValue("@Flg_Calc_Costo_Tela_Acabada", req.Parametros["Flg_Calc_Costo_Tela_Acabada"]);

                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }

             
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reporte");
                    worksheet.Cell(1, 1).InsertTable(dataTable);
 
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteGerencial.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Ahora, el error será más descriptivo y te dirá exactamente qué falla
                return StatusCode(500, new { error = "Error al generar el reporte: " + ex.Message });
            }
        }

        // =================================================================================
        // 4. GENERAR EXCEL PLAN CONTABLE V2 (El reporte específico que te falta)
        // Ruta: /api/sql/generate-excel-formato-plancontable_v2
        // =================================================================================
        [HttpPost("generate-excel-formato-plancontable_v2")]
        public async Task<IActionResult> GenerateExcelFormatoPlanContablev2([FromBody] ProcedureRequest req)
        {
            try
            {
                var dataTable = new DataTable();
                var datoaño = new DataTable();

                // 1. Obtener Datos del Grid (Tu SP Principal)
                using (var conn = GetConnection())
                {
                    using (var cmd = new SqlCommand(req.Nombre, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0; // Sin límite de tiempo

                        if (req.Parametros != null)
                        {
                            foreach (var kv in req.Parametros)
                            {
                                // Usamos la misma lógica simple que en tus otros métodos
                                cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value ?? "");
                            }
                        }

                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }

                // 2. Obtener Datos de Cabecera (Empresa/Año) - SP Fijo
                using (var conn = GetConnection())
                {
                    using (var cmd = new SqlCommand("EJERCICIO_EMPRESA_AÑO", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Option", "1"); // Opción fija para cabecera

                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            datoaño.Load(reader);
                        }
                    }
                }

                // 3. Dibujar el Excel con ClosedXML (Diseño específico)
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Plan Contable");

                    // Datos de cabecera seguros
                    var primerRuc = datoaño.Rows.Count > 0 ? datoaño.Rows[0]["ruc"].ToString() : "";
                    var primerAño = datoaño.Rows.Count > 0 ? datoaño.Rows[0]["Año_cont"].ToString() : "";
                    var primerEmpresa = datoaño.Rows.Count > 0 ? datoaño.Rows[0]["Empresa"].ToString() : "";

                    // --- DISEÑO VISUAL ---
                    // Cabecera
                    worksheet.Cell("A1").Value = "PLAN GENERAL DE CUENTAS";
                    worksheet.Range("A1:E1").Merge().Style.Font.Bold = true;
                    worksheet.Range("A1:E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("A2").Value = "RAZON SOCIAL: " + primerEmpresa;
                    worksheet.Range("A2:E2").Merge();

                    worksheet.Cell("A3").Value = "RUC: " + primerRuc;
                    worksheet.Range("A3:E3").Merge();

                    worksheet.Cell("A4").Value = "EJERCICIO: " + primerAño;
                    worksheet.Range("A4:E4").Merge();

                    // Insertamos los datos empezando en la fila 6
                    if (dataTable.Rows.Count > 0)
                    {
                        var table = worksheet.Cell(6, 1).InsertTable(dataTable);
                        table.Theme = XLTableTheme.None; // Sin estilos automáticos para que no dañe tu diseño
                    }

                    // Ajustar columnas
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "PlanContable_V2.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error Plan Contable V2: " + ex.Message });
            }
        }

        // =================================================================================
        // 5. EXPORTAR TABLA DE CIERRE (NUEVO)
        // Ruta: /api/sql/procedure/exportar-cierre
        // =================================================================================
        [HttpGet("procedure/exportar-cierre")]
        public async Task<IActionResult> ExportarTablaCierre([FromQuery] string asiento = "")
        {
            try
            {
                var dataTable = new DataTable();

                using (var conn = GetConnection())
                {
                    // Usamos el SP de consulta que ya tienes
                    using (var cmd = new SqlCommand("sp_Mantenimiento_Tabla_Cierre", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Si viene un asiento, lo mandamos. Si no, mandamos vacío para que traiga todo.
                        if (!string.IsNullOrEmpty(asiento))
                        {
                            // IMPORTANTE: Asegúrate de que tu SP de listado acepte este filtro
                            // Si tu SP de listado NO filtra por asiento, tendrás que filtrar en memoria (C#)
                            // cmd.Parameters.AddWithValue("@Asiento", asiento); 
                        }

                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }

                // Filtrado en memoria por si el SP trae todo (Más seguro para no modificar SQL)
                if (!string.IsNullOrEmpty(asiento))
                {
                    var filasFiltradas = dataTable.AsEnumerable()
                        .Where(row => row["Asiento"].ToString() == asiento);

                    if (filasFiltradas.Any())
                    {
                        dataTable = filasFiltradas.CopyToDataTable();
                    }
                    else
                    {
                        dataTable.Rows.Clear(); // Si no hay coincidencias, devolvemos vacío
                    }
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Cierre");

                    // Cabecera Simple
                    worksheet.Cell("A1").Value = "REPORTE DE TABLA DE CIERRE";
                    worksheet.Range("A1:D1").Merge().Style.Font.Bold = true;

                    // Insertar Datos
                    if (dataTable.Rows.Count > 0)
                    {
                        worksheet.Cell(3, 1).InsertTable(dataTable);
                    }
                    else
                    {
                        worksheet.Cell(3, 1).Value = "No se encontraron registros para el filtro solicitado.";
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"TablaCierre_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error exportando cierre: " + ex.Message });
            }
        }
    }

    public class ProcedureRequest
    {
        public string Nombre { get; set; } = "";
        public Dictionary<string, string>? Parametros { get; set; }
    }
}

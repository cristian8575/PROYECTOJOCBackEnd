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
            return new SqlConnection(_config.GetConnectionString("Default"));
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
                        row[reader.GetName(i)] = reader.GetValue(i);
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

 

    }

    public class ProcedureRequest
    {
        public string Nombre { get; set; } = "";
        public Dictionary<string, string>? Parametros { get; set; }
    }
}

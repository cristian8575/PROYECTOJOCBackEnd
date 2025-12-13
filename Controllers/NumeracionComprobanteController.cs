using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TEXTILJOC_ConcarWeb.Data;
using TEXTILJOC_ConcarWeb.Models;

namespace TEXTILJOC_ConcarWeb.Controllers
{
    [Route("api/numeracioncomprobante")]
    [ApiController]
    public class NumeracionComprobanteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NumeracionComprobanteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("listar")]
        public async Task<IActionResult> Listar([FromBody] NumeracionComprobanteRequest req)
        {
            return await ExecuteSp("LISTAR", req);
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> Guardar([FromBody] NumeracionComprobanteRequest req)
        {
            return await ExecuteSp("GUARDAR", req);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] NumeracionComprobanteRequest req)
        {
            return await ExecuteSp("ELIMINAR", req);
        }

        [HttpGet("subdiarios")]
        public async Task<IActionResult> ListarSubdiarios()
        {
            var result = new List<dynamic>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCAR_LISTARSUBDIARIOS"; // Assuming this User provided SP exists
                    command.CommandType = CommandType.StoredProcedure;

                    var pAccion = command.CreateParameter();
                    pAccion.ParameterName = "@Accion";
                    pAccion.Value = "LISTAR_SUBDIARIOS";
                    command.Parameters.Add(pAccion);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                codigo = reader["Codigo"]?.ToString(),
                                descripcion = reader["Descripcion"]?.ToString()
                            });
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error cargando subdiarios: " + ex.Message });
            }
        }

        private async Task<IActionResult> ExecuteSp(string accion, NumeracionComprobanteRequest req)
        {
            var connection = _context.Database.GetDbConnection();
            string mensaje = "";
            string resultado = "ERROR";

            try
            {
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCAR_NUMERACION_COMPROBANTE_CRUD";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Accion", accion));
                    // Handle empty strings as DBNull for proper filtering/insertion
                    command.Parameters.Add(new SqlParameter("@Cod_Subdiario", string.IsNullOrWhiteSpace(req.Cod_Subdiario) ? DBNull.Value : req.Cod_Subdiario));
                    command.Parameters.Add(new SqlParameter("@Anio", string.IsNullOrWhiteSpace(req.Anio) ? DBNull.Value : req.Anio));
                    command.Parameters.Add(new SqlParameter("@Mes", string.IsNullOrWhiteSpace(req.Mes) ? DBNull.Value : req.Mes));
                    command.Parameters.Add(new SqlParameter("@Ult_Comprobante", string.IsNullOrWhiteSpace(req.Ult_Comprobante) ? DBNull.Value : req.Ult_Comprobante));
                    command.Parameters.Add(new SqlParameter("@Ult_Comprobante_Real", string.IsNullOrWhiteSpace(req.Ult_Comprobante_Real) ? DBNull.Value : req.Ult_Comprobante_Real));
                    command.Parameters.Add(new SqlParameter("@Cod_Usuario", string.IsNullOrWhiteSpace(req.Cod_Usuario) ? DBNull.Value : req.Cod_Usuario));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (accion == "LISTAR")
                        {
                            var list = new List<Dictionary<string, object?>>();
                            while (await reader.ReadAsync())
                            {
                                var dict = new Dictionary<string, object?>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var val = reader.GetValue(i);
                                    dict[reader.GetName(i)] = val == DBNull.Value ? null : val;
                                }
                                list.Add(dict);
                            }
                            return Ok(list);
                        }
                        else
                        {
                            if (await reader.ReadAsync())
                            {
                                // Columns are usually Result / Mensaje
                                try { resultado = reader["Resultado"]?.ToString() ?? "ERROR"; } catch { }
                                try { mensaje = reader["Mensaje"]?.ToString() ?? ""; } catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en servidor: " + ex.Message });
            }

            if (accion != "LISTAR")
            {
                if (resultado == "OK")
                    return Ok(new { message = mensaje });
                else
                    return StatusCode(500, new { message = mensaje });
            }

            return Ok();
        }
    }
}

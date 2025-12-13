using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TEXTILJOC_ConcarWeb.Data;
using TEXTILJOC_ConcarWeb.Models;

namespace TEXTILJOC_ConcarWeb.Controllers
{
    [Route("api/mantenimientobancos")]
    [ApiController]
    public class MantenimientoCuentasBancosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MantenimientoCuentasBancosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("listar")]
        public async Task<IActionResult> Listar([FromBody] MantenimientoCuentasBancosRequest req)
        {
            return await ExecuteSp("LISTAR", req);
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> Guardar([FromBody] MantenimientoCuentasBancosRequest req)
        {
            return await ExecuteSp("GUARDAR", req);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] MantenimientoCuentasBancosRequest req)
        {
            return await ExecuteSp("ELIMINAR", req);
        }

        // Endpoint para cargar los comboboxes (Moneda, Modelo, Entidad, TipoCuenta)
        [HttpGet("combos/{tabla}")]
        public async Task<IActionResult> GetCombos(string tabla)
        {
            var result = new List<dynamic>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCARCOMBO_MANT_BANCOS";
                    command.CommandType = CommandType.StoredProcedure;

                    var pTabla = command.CreateParameter();
                    pTabla.ParameterName = "@Tabla";
                    pTabla.Value = tabla;
                    command.Parameters.Add(pTabla);

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
                return StatusCode(500, new { message = "Error cargando combos: " + ex.Message });
            }
        }

        private string? SafeString(string? val, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(val)) return null;
            val = val.Trim();
            return val.Length > maxLength ? val.Substring(0, maxLength) : val;
        }

        private async Task<IActionResult> ExecuteSp(string accion, MantenimientoCuentasBancosRequest req)
        {
            var connection = _context.Database.GetDbConnection();
            string mensaje = "";
            string resultado = "ERROR";

            try
            {
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCAR_MANTENIMIENTOS_BANCOS_CRUD";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Accion", accion));
                    command.Parameters.Add(new SqlParameter("@ID", (object?)req.ID ?? DBNull.Value));

                    // Apply SafeString to enforce DB limits
                    command.Parameters.Add(new SqlParameter("@Cod_Cuenta", (object?)SafeString(req.Cod_Cuenta, 20) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Descripcion", (object?)SafeString(req.Descripcion, 100) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Nombre_Banco", (object?)SafeString(req.Nombre_Banco, 100) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Numero_Cuenta", (object?)SafeString(req.Numero_Cuenta, 50) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Id_Moneda", (object?)SafeString(req.Id_Moneda, 4) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Mod_EstadoCuenta", (object?)SafeString(req.Mod_EstadoCuenta, 10) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Ent_Financiera", (object?)SafeString(req.Ent_Financiera, 5) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Tipo_Cuenta_Reporte", (object?)SafeString(req.Tipo_Cuenta_Reporte, 8) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Orden_Reporte", (object?)SafeString(req.Orden_Reporte, 10) ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cuenta_Contable", (object?)SafeString(req.Cuenta_Contable, 20) ?? DBNull.Value));

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
                                try
                                {
                                    mensaje = reader["Mensaje"]?.ToString() ?? "";
                                    resultado = reader["Resultado"]?.ToString() ?? "ERROR";
                                }
                                catch
                                {
                                    mensaje = "Respuesta desconocida del servidor";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Enriquecer el mensaje de error con los datos enviados para facilitar depuración
                string debugInfo = $" | Moneda: '{req.Id_Moneda}', Banco: '{req.Mod_EstadoCuenta}', Entidad: '{req.Ent_Financiera}'";
                return StatusCode(500, new { message = "Error interno: " + ex.Message + debugInfo });
            }

            if (resultado == "OK")
                return Ok(new { message = mensaje });
            else
                return StatusCode(500, new { message = mensaje });
        }
    }
}

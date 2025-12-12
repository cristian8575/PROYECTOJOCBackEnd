using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using TEXTILJOC_ConcarWeb.Data;
using TEXTILJOC_ConcarWeb.Models;

namespace TEXTILJOC_ConcarWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnexosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnexosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("guardar")]
        public async Task<IActionResult> Guardar([FromBody] AnexoRequest req)
        {
            return await ExecuteSp("GUARDAR", req);
        }

        [HttpPost("eliminar")]
        public async Task<IActionResult> Eliminar([FromBody] AnexoRequest req)
        {
            return await ExecuteSp("ELIMINAR", req);
        }

        [HttpPost("consultar")]
        public async Task<IActionResult> Consultar([FromBody] AnexoRequest req)
        {
            // For query, we might want to return the dataset directly, 
            // but the ExecuteSp helper below handles 'Result' messages.
            // If the user wants the data back, we need a different approach.
            // Assuming this is for basic CRUD check or single item load.
            // For now, reusing the logic.
            return await ExecuteSp("CONSULTAR", req);
        }

        private async Task<IActionResult> ExecuteSp(string accion, AnexoRequest req)
        {
            var connection = _context.Database.GetDbConnection();
            string mensaje = "";
            string resultado = "ERROR";

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SP_CONCAR_ANEXOS_CRUD";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Accion", accion));

                    // Mapeo manual de todos los parámetros
                    command.Parameters.Add(new SqlParameter("@Cod_TipAnex", (object?)req.Cod_TipAnex ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Anxo", (object?)req.Cod_Anxo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Des_Anexo", (object?)req.Des_Anexo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Num_Ruc", (object?)req.Num_Ruc ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Dir_Anexo", (object?)req.Dir_Anexo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_TipoPersona", (object?)req.Cod_TipoPersona ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Apellido_Paterno", (object?)req.Apellido_Paterno ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Apellido_Materno", (object?)req.Apellido_Materno ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Nombres", (object?)req.Nombres ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_TipoDoc", (object?)req.Cod_TipoDoc ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Telefono", (object?)req.Telefono ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Correo", (object?)req.Correo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Ubigeo", (object?)req.Cod_Ubigeo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@EntidadFinanciera", (object?)req.EntidadFinanciera ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Sexo", (object?)req.Cod_Sexo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@FechaNacimiento", (object?)req.FechaNacimiento ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@EssaludVida", (object?)req.EssaludVida ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@TipoComision", (object?)req.TipoComision ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cuspp", (object?)req.Cuspp ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@NombreVia", (object?)req.NombreVia ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@NumeroAnexo", (object?)req.NumeroAnexo ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Interior", (object?)req.Interior ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Zona", (object?)req.Cod_Zona ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@NombreZona", (object?)req.NombreZona ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@ReferenciaZona", (object?)req.ReferenciaZona ?? DBNull.Value));

                    command.Parameters.Add(new SqlParameter("@Es_Nacional", (object?)req.Es_Nacional ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Es_Extranjero", (object?)req.Es_Extranjero ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Es_Vigente", (object?)req.Es_Vigente ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Es_Anulado", (object?)req.Es_Anulado ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Es_Domiciliado", (object?)req.Es_Domiciliado ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@AplicaConvenio", (object?)req.AplicaConvenio ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@FormSuspension", (object?)req.FormSuspension ?? DBNull.Value));

                    command.Parameters.Add(new SqlParameter("@Cod_Situacion", (object?)req.Cod_Situacion ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Nacionalidad", (object?)req.Cod_Nacionalidad ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_TipoVia", (object?)req.Cod_TipoVia ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_Moneda", (object?)req.Cod_Moneda ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Flag_Essalud", (object?)req.Flag_Essalud ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cod_TipoComision", (object?)req.Cod_TipoComision ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Formulario_Susp", (object?)req.Formulario_Susp ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Tasa_Detraccion", (object?)req.Tasa_Detraccion ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Tasa_Percepcion", (object?)req.Tasa_Percepcion ?? DBNull.Value));

                    // Extras
                    command.Parameters.Add(new SqlParameter("@Dir_FACELE", (object?)req.NombreVia ?? DBNull.Value)); // Reuse NombreVia or map separately if needed


                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (accion == "CONSULTAR")
                        {
                            // Load DataTable or list for Consult output
                            var dt = new DataTable();
                            dt.Load(reader);
                            // Return list of dictionaries? Or just the list
                            return Ok(dt);
                        }

                        if (await reader.ReadAsync())
                        {
                            // Check for success column 'Msg'
                            try
                            {
                                mensaje = reader["Msg"]?.ToString() ?? "Sin respuesta";
                            }
                            catch (IndexOutOfRangeException)
                            {
                                // If 'Msg' not found, check for 'Mensaje' from Catch block
                                try
                                {
                                    var sqlError = reader["Mensaje"]?.ToString();
                                    var errType = reader["Error"]?.ToString();
                                    mensaje = $"ERROR SQL: {errType} - {sqlError}";
                                    resultado = "ERROR";
                                }
                                catch
                                {
                                    mensaje = "Error desconocido en respuesta del SP";
                                    resultado = "ERROR";
                                }
                            }

                            if (mensaje.StartsWith("OK")) resultado = "OK";
                            else if (mensaje.StartsWith("ERROR")) resultado = "ERROR";
                            else resultado = "OK";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            if (resultado == "OK")
                return Ok(new { value = mensaje });
            else
                return StatusCode(500, new { value = mensaje });
        }
    }
}

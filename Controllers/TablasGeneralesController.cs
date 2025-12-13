using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using ClosedXML.Excel; // Asegúrate de tener instalado ClosedXML
using System.IO;

namespace TuProyecto.Controllers
{
	[Route("api/[controller]")] // Ruta: api/TablasGenerales
	[ApiController]
	public class TablasGeneralesController : ControllerBase
	{
		private readonly string _cadenaSQL;

		public TablasGeneralesController(IConfiguration config)
		{
			_cadenaSQL = config.GetConnectionString("CadenaSQL");
		}

		// GET: api/TablasGenerales/exportar/05
		[HttpGet("exportar/{codigo}")]
		public IActionResult ExportarExcel(string codigo)
		{
			DataTable dt = new DataTable();

			// Le ponemos un nombre genérico, luego el Front le pone el nombre bonito al archivo
			dt.TableName = "Datos";

			try
			{
				using (var conexion = new SqlConnection(_cadenaSQL))
				{
					conexion.Open();

					// --- CAMBIO CLAVE: Usamos tu SP Maestro ---
					using (var cmd = new SqlCommand("sp_Concar_Obtener_Data_Para_Excel", conexion))
					{
						cmd.CommandType = CommandType.StoredProcedure;

						// El parámetro en tu SP se llama @CodigoTabla
						cmd.Parameters.AddWithValue("@CodigoTabla", codigo);

						using (var adapter = new SqlDataAdapter(cmd))
						{
							// Esto llenará el DataTable con las columnas que tenga esa tabla específica
							adapter.Fill(dt);
						}
					}
				}

				if (dt.Rows.Count == 0)
				{
					// Opcional: Puedes retornar NotFound o descargar un Excel vacío con cabeceras
					// return NotFound("La tabla seleccionada no contiene datos o no existe.");
				}

				// Generación del Excel con ClosedXML
				using (var workbook = new XLWorkbook())
				{
					var worksheet = workbook.Worksheets.Add(dt);

					// Ajuste visual automático
					worksheet.Columns().AdjustToContents();

					using (var stream = new MemoryStream())
					{
						workbook.SaveAs(stream);
						var content = stream.ToArray();

						return File(
							content,
							"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
							$"Reporte_{codigo}.xlsx"
						);
					}
				}
			}
			catch (Exception ex)
			{
				// Si el SP falla (por ejemplo, si la tabla no existe en la BD), caerá aquí
				return StatusCode(500, $"Error generando Excel: {ex.Message}");
			}
		}
	}
}
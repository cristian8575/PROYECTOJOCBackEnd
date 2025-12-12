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
    public class TipoCambioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TipoCambioController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("moneda")]
        public async Task<IActionResult> GetMonedas()
        {
            var result = new List<MonedaDto>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CONCAR_MANT_TIPOCAMBIO";
                    command.CommandType = CommandType.StoredProcedure;

                    var paramAccion = command.CreateParameter();
                    paramAccion.ParameterName = "@ACCION";
                    paramAccion.Value = "LISTAR_MONEDAS";
                    command.Parameters.Add(paramAccion);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new MonedaDto
                            {
                                Clave = reader["CLAVE"]?.ToString() ?? "",
                                Descripcion = reader["DESCRIPCION"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return Ok(result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList(string moneda, string anio, string mes)
        {
            var result = new List<TipoCambioDto>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CONCAR_MANT_TIPOCAMBIO";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ACCION", "LISTAR"));
                    command.Parameters.Add(new SqlParameter("@CODMONEDA", (object?)moneda ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@ANIO", (object?)anio ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@MES", (object?)mes ?? DBNull.Value));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new TipoCambioDto
                            {
                                Dia = $"{reader["Dia"]?.ToString()?.PadLeft(2, '0')}/{mes}/{anio}",
                                TipoCambioCompra = Convert.ToDecimal(reader["TipoCambioCompra"] ?? 0),
                                TipoCambioVenta = Convert.ToDecimal(reader["TipoCambioVenta"] ?? 0),
                                FecCreacion = reader["FecCreacion"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            return Ok(result);
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] TipoCambioRequest req)
        {
            var connection = _context.Database.GetDbConnection();
            string mensaje = "";
            string resultado = "ERROR";

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CONCAR_MANT_TIPOCAMBIO";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ACCION", "GUARDAR"));
                    command.Parameters.Add(new SqlParameter("@CODMONEDA", req.Moneda));
                    command.Parameters.Add(new SqlParameter("@ANIO", req.Anio));
                    command.Parameters.Add(new SqlParameter("@MES", req.Mes));
                    command.Parameters.Add(new SqlParameter("@DIA", req.Dia));
                    command.Parameters.Add(new SqlParameter("@COMPRA", req.Compra));
                    command.Parameters.Add(new SqlParameter("@VENTA", req.Venta));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resultado = reader["Resultado"]?.ToString() ?? "ERROR";
                            mensaje = reader["Mensaje"]?.ToString() ?? "Sin respuesta del servidor";
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            if (resultado == "OK")
                return Ok(new { message = mensaje });
            else
                return StatusCode(500, new { message = mensaje });
        }

        [HttpPost("insert-masivo")]
        public async Task<IActionResult> InsertMasivo([FromBody] TipoCambioMasivoRequest req)
        {
            var connection = _context.Database.GetDbConnection();
            string mensaje = "";
            string resultado = "ERROR";

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "CONCAR_MANT_TIPOCAMBIO";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@ACCION", "GUARDAR_MASIVO"));
                    command.Parameters.Add(new SqlParameter("@CODMONEDA", req.Moneda));
                    command.Parameters.Add(new SqlParameter("@ANIO", req.Anio));
                    command.Parameters.Add(new SqlParameter("@MES", req.Mes));
                    command.Parameters.Add(new SqlParameter("@DIA", req.DiaDesde));
                    command.Parameters.Add(new SqlParameter("@DIA_FIN", req.DiaHasta));
                    command.Parameters.Add(new SqlParameter("@COMPRA", req.Compra));
                    command.Parameters.Add(new SqlParameter("@VENTA", req.Venta));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            resultado = reader["Resultado"]?.ToString() ?? "ERROR";
                            mensaje = reader["Mensaje"]?.ToString() ?? "Sin respuesta del servidor";
                        }
                    }
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    await connection.CloseAsync();
            }

            if (resultado == "OK")
                return Ok(new { message = mensaje });
            else
                return StatusCode(500, new { message = mensaje });
        }
    }
}
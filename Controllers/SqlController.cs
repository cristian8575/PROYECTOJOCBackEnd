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
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Error en SP", detalle = ex.Message });
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

                     
                        DateTime fechaIni = DateTime.ParseExact((req.Parametros?["FECHA"] ?? ""), "dd/MM/yyyy", null);
                        DateTime fechaFin = DateTime.ParseExact((req.Parametros?["FECHA1"] ?? ""), "dd/MM/yyyy", null);

                        cmd.Parameters.AddWithValue("@FECHA", fechaIni);
                        cmd.Parameters.AddWithValue("@FECHA1", fechaFin);
                        cmd.Parameters.AddWithValue("@FLG_REPROCESO", req.Parametros["FLG_REPROCESO"]);
                        cmd.Parameters.AddWithValue("@OPCION", req.Parametros["OPCION"]);
                        cmd.Parameters.AddWithValue("@Cod_GamCol", req.Parametros["Cod_GamCol"]);
                        cmd.Parameters.AddWithValue("@Cod_TipoReceta", req.Parametros["Cod_TipoReceta"])
                            
                            
                            
                            
                            
                            
                            ;
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
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteGerencial_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx"
);
                    }
                }
            }
            catch (Exception ex)
            {
                // Ahora, el error será más descriptivo y te dirá exactamente qué falla
                return StatusCode(500, new { error = "Error al generar el reporte: " + ex.Message });
            }
        }




        [HttpPost("generate-excel-formato-plancontable")]
        public async Task<IActionResult> GenerateExcelFormatoPlanContable([FromBody] ProcedureRequest req)
        {
            try
            {
     
                var dataTable = new DataTable();
                var datoaño = new DataTable();
                using (var conn = GetConnection())
                {
                    using (var cmd = new SqlCommand(req.Nombre, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 0;
                        cmd.Parameters.AddWithValue("@Option", ((object)req.Parametros?["Option"] ?? DBNull.Value));
                        cmd.Parameters.AddWithValue("@Cod_ctacontini", ((object)req.Parametros?["Cod_ctacontini"] ?? DBNull.Value));
                        cmd.Parameters.AddWithValue("@Cod_ctacontfin", ((object)req.Parametros?["Cod_ctacontfin"] ?? DBNull.Value));
                         
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
                


                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Plan Contable");

                    worksheet.Cell(1, 1).Value = "Campo";
                    worksheet.Cell(1, 2).Value = "Cuenta";
                    worksheet.Cell(1, 3).Value = "Descripcion";
                    worksheet.Cell(1, 4).Value = "Tipo de Cuenta";
                    worksheet.Cell(1, 5).Value = "Nivel de Saldo";
                    worksheet.Cell(1, 6).Value = "Tipo de Anexo";
                    worksheet.Cell(1, 7).Value = "Acepta Centro Costo";
                    worksheet.Cell(1, 8).Value = "Documento de Referencia";
                    worksheet.Cell(1, 9).Value = "Fecha de Vencimiento";
                    worksheet.Cell(1,10).Value = "REG.CTA.";
                    worksheet.Cell(1,11).Value = "Moneda";
                    worksheet.Cell(1,12).Value = "Cuenta Cargo";
                    worksheet.Cell(1,13).Value = "Cuenta Abono";
                    worksheet.Cell(1,14).Value = "Balance General";
                    worksheet.Cell(1,15).Value = "G Y Perd. por Funcion";
                    worksheet.Cell(1,16).Value = "G Y Perd. por Naturaleza";
                    worksheet.Cell(1,17).Value = "Centro Costo";

                    var headerRange = worksheet.Range("A1:Q1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    worksheet.Cell(2, 1).Value = "Restricciones";
                    worksheet.Cell(2, 2).Value = "SOLO NUMEROS";
                    worksheet.Cell(2, 3).Value = "";
                    worksheet.Cell(2, 4).Value = "Especificar solo los Tipo Cta. De la tabla general 08";
                    worksheet.Cell(2, 5).Value = "Ingresar Nivel de saldo (1,2,3)";
                    worksheet.Cell(2, 6).Value = "Especificar solo los Tipo Anexo De la tabla General 07";
                    worksheet.Cell(2, 7).Value = "Ingresar(S o N)";
                    worksheet.Cell(2, 8).Value = "Ingresar(S o N)";
                    worksheet.Cell(2, 9).Value = "Ingresar(S o N)";
                    worksheet.Cell(2, 10).Value = "Ingresar(M o A)";
                    worksheet.Cell(2, 11).Value = "Ingresar Tipo Moneda (MN o US)";
                    worksheet.Cell(2, 12).Value = "Verificar que exista en el campo Cuenta";
                    worksheet.Cell(2, 13).Value = "Verificar que exista en el campo Cuenta";
                    worksheet.Cell(2, 14).Value = "Especificar solo los formatos de Balance de la tabla General 10";
                    worksheet.Cell(2, 15).Value = "Especificar solo los formatos de G y Perd. Por Funcion de la Tabla General 11";
                    worksheet.Cell(2, 16).Value = "Especificar solo los formatos de G y Perd. Por Naturaleza de la Tabla General 13";
                    worksheet.Cell(2, 17).Value = "Es obligatorio si el valor la columna G(ACEPTA CENTRO COSTO) esta es S Validar con la Tabla General 12";
                    var restricRange = worksheet.Range("A2:Q2");
                    restricRange.Style.Fill.BackgroundColor = XLColor.Yellow;
                    restricRange.Style.Alignment.WrapText = true;
                    restricRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    restricRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    restricRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    restricRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    worksheet.Cell(3, 1).Value = "Tamaño/Formato";
                    worksheet.Cell(3, 2).Value = "8 Caracteres";
                    worksheet.Cell(3, 3).Value = "50 Caracteres";
                    worksheet.Cell(3, 4).Value = "1 Caracter";
                    worksheet.Cell(3, 5).Value = "1 Caracter";
                    worksheet.Cell(3, 6).Value = "1 Caracter";
                    worksheet.Cell(3, 7).Value = "1 Caracter";
                    worksheet.Cell(3, 8).Value = "1 Caracter";
                    worksheet.Cell(3, 9).Value = "1 Caracter";
                    worksheet.Cell(3, 10).Value = "1 Caracter";
                    worksheet.Cell(3, 11).Value = "2 Caracter";
                    worksheet.Cell(3, 12).Value = "8 Caracter";
                    worksheet.Cell(3, 13).Value = "8 Caracter";
                    worksheet.Cell(3, 14).Value = "4 Caracter";
                    worksheet.Cell(3, 15).Value = "4 Caracter";
                    worksheet.Cell(3, 16).Value = "4 Caracter";
                    worksheet.Cell(3, 17).Value = "4 Caracter";
                    var sizeRange = worksheet.Range("A3:Q3");
                    sizeRange.Style.Font.Italic = true;
                    sizeRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    sizeRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    sizeRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    worksheet.Cell(4, 1).Value = "Valores/Validos";
                    worksheet.Cell(4, 2).Value = " ";
                    worksheet.Cell(4, 3).Value = " ";
                    worksheet.Cell(4, 4).Value = "Valores...";
                    worksheet.Cell(4, 5).Value = "Valores...";
                    worksheet.Cell(4, 6).Value = "Valores...";
                    worksheet.Cell(4, 7).Value = "Valores...";
                    worksheet.Cell(4, 8).Value = "Valores...";
                    worksheet.Cell(4, 9).Value = "Valores...";
                    worksheet.Cell(4, 10).Value = "Valores...";
                    worksheet.Cell(4, 11).Value = "Valores...";
                    worksheet.Cell(4, 12).Value = " ";
                    worksheet.Cell(4, 13).Value = " ";
                    worksheet.Cell(4, 14).Value = "Valores...";
                    worksheet.Cell(4, 15).Value = "Valores...";
                    worksheet.Cell(4, 16).Value = "Valores...";
                    worksheet.Cell(4, 17).Value = "Valores...";
                    var valoresRange = worksheet.Range("A4:Q4");
                    valoresRange.Style.Font.FontColor = XLColor.Gray;
                    valoresRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    valoresRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    valoresRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
 

                    // Insertar solo las columnas que quieras mostrar
                    var data = dataTable.AsEnumerable()
                        .Select(row => new
                        {
                            Campo = "",
                            Cuenta = row["Cod_CtaCont"].ToString(),
                            Descripcion = row["Des_CtaCont"].ToString(),
                            Tipo = row["Tip_Cuenta"].ToString(),
                            Nivel = row["Nivel_Saldo"].ToString(),
                            Anexo = row["Tipo_Anexo"].ToString(),
                            Flg_AceptaCentro_Costo = row["Flg_AceptaCentro_Costo"].ToString(),
                            DocRef = row["Documento_Referencia"].ToString(),
                            Fecha_vencimiento = row["Fecha_vencimiento"].ToString(),
                            Reg_CTA = row["Reg_CTA"].ToString(),
                            Moneda = row["Moneda"].ToString(),
                            Cuenta_Cargo = row["Cuenta_Cargo"].ToString(),
                            Cuenta_Abono = row["Cuenta_Abono"].ToString(),
                            Balance_General = row["Balance_General"].ToString(),
                            Gan_Perdi_por_funcion = row["Gan_Perdi_por_funcion"].ToString(),
                            Gan_Perdi_por_naturaleza = row["Gan_Perdi_por_naturaleza"].ToString(),
                            Centro_Costo = row["Centro_Costo"].ToString(),
                           



                        });

                    

                    worksheet.Cell(5, 1).InsertData(data);
                  
                    

                    var lastRow = (worksheet.LastRowUsed()?.RowNumber() ?? 0);
                    var tableRange = worksheet.Range($"A5:Q{lastRow}");
                    tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    worksheet.Column(1).Width = 12;
                    worksheet.Column(2).Width = 15;
                    worksheet.Column(3).Width = 50;
                    worksheet.Column(4).Width = 20;
                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 15;
                    worksheet.Column(7).Width = 20;
                    worksheet.Column(8).Width = 22;
                    worksheet.Column(9).Width = 22;
                    worksheet.Column(10).Width = 12;
                    worksheet.Column(11).Width = 12;
                    worksheet.Column(12).Width = 18;
                    worksheet.Column(13).Width = 18;
                    worksheet.Column(14).Width = 20;
                    worksheet.Column(15).Width = 22;
                    worksheet.Column(16).Width = 22;
                    worksheet.Column(17).Width = 20;

 
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "ReporteGerencial.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                // Ahora, el error será más descriptivo y te dirá exactamente qué falla
                return StatusCode(500, new { error = "Error al generar el reporte: " + ex.Message });
            }
        }


        [HttpPost("generate-excel-formato-plancontable_v2")]
        public async Task<IActionResult> GenerateExcelFormatoPlanContablev2([FromBody] ProcedureRequest req)
         
        {

            var dataTable = new DataTable();
            var datoaño = new DataTable();
            using (var conn = GetConnection())
            {
                using (var cmd = new SqlCommand(req.Nombre, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Option", ((object)req.Parametros?["Option"] ?? DBNull.Value));
                    cmd.Parameters.AddWithValue("@Cod_ctacontini", ((object)req.Parametros?["Cod_ctacontini"] ?? DBNull.Value));
                    cmd.Parameters.AddWithValue("@Cod_ctacontfin", ((object)req.Parametros?["Cod_ctacontfin"] ?? DBNull.Value));

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dataTable.Load(reader);
                    }
                }
            }

            using (var conn = GetConnection())
            {
                using (var cmd = new SqlCommand("EJERCICIO_EMPRESA_AÑO", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Option", "1");

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        datoaño.Load(reader);
                    }
                }
            }


            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Plan Contable");

               
                var primerRuc = datoaño.Rows[0]["ruc"].ToString();
                var primerAño = datoaño.Rows[0]["Año_cont"].ToString();
                var primerEmpresa = datoaño.Rows[0]["Empresa"].ToString(); 
 
                // Cabecera
                worksheet.Cell("A1").Value = "PLAN GENERAL DE CUENTAS";
                worksheet.Cell("A2").Value = "RAZON SOCIAL:"+ primerEmpresa +" "+ primerAño;
                worksheet.Cell("A3").Value = "RUC: "+ primerRuc;
                worksheet.Cell("A4").Value = "Ejercicio: "+ primerAño;
                worksheet.Cell("A5").Value = "CTPLAN Fecha: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                worksheet.Range("A1:J1").Merge().Style.Font.Bold = true;
                worksheet.Range("A1:J1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Encabezados de la tabla
                worksheet.Cell("A7").Value = "CUENTA";
                worksheet.Cell("B7").Value = "DESCRIPCIÓN";
                worksheet.Cell("C7").Value = "TIP. CTA.";
                worksheet.Cell("D7").Value = "NIV. SAL.";
                worksheet.Cell("E7").Value = "ANE";
                worksheet.Cell("F7").Value = "ANE. REF.";
                worksheet.Cell("G7").Value = "CEN. COS.";
                worksheet.Cell("H7").Value = "DOC. REF.";
                worksheet.Cell("I7").Value = "DOC. REF.2";
                worksheet.Cell("J7").Value = "FEC. VEN.";
                worksheet.Cell("K7").Value = "ARE";
                worksheet.Cell("L7").Value = "REG. CTA.";
                worksheet.Cell("M7").Value = "MON.";
                worksheet.Cell("N7").Value = "CON. BAN.";
                worksheet.Cell("O7").Value = "MED. PAGO";
                worksheet.Cell("P7").Value = "CUENTA CARGO";
                worksheet.Cell("Q7").Value = "CUENTA ABONO";
                worksheet.Cell("R7").Value = "Balance General";
                worksheet.Cell("S7").Value = "G y Perd. por Funcion";
                worksheet.Cell("T7").Value = "G y Perd. por Naturaleza";
                worksheet.Cell("U7").Value = "Costos";
                worksheet.Cell("V7").Value = "Ingresos y Gastos";
                worksheet.Cell("W7").Value = "Costos";
                worksheet.Cell("X7").Value = "Balance General";
                worksheet.Cell("Y7").Value = "G y Perd. por Funcion";
                worksheet.Cell("Z7").Value = "G y Perd por Naturaleza";

                // ... resto de columnas
                var valoresRange = worksheet.Range("A7:Z7");
              
                valoresRange.Style.Alignment.WrapText = true;
                valoresRange.Style.Font.Bold = true;
                valoresRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                valoresRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                valoresRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                valoresRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                // Simulación de datos
                worksheet.Range("R6:U6").Merge();
                worksheet.Cell(6, 18).Value = "Estandares";
                worksheet.Range("V6:Z6").Merge();
                worksheet.Cell(6, 22).Value = "Alternos";
                var valoreTOPRange = worksheet.Range("R6:Z6");
                valoreTOPRange.Style.Font.Bold = true;
                valoreTOPRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                valoreTOPRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                valoreTOPRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Column(1).Width = 15;
                worksheet.Column(2).Width = 50;
                worksheet.Column(3).Width = 6;
                worksheet.Column(4).Width = 6;
                worksheet.Column(5).Width = 6;
                worksheet.Column(6).Width = 6;
                worksheet.Column(7).Width = 6;
                worksheet.Column(8).Width = 6;
                worksheet.Column(9).Width = 6;
                worksheet.Column(10).Width = 6;
                worksheet.Column(11).Width = 6;
                worksheet.Column(12).Width = 6;
                worksheet.Column(13).Width = 6;
                worksheet.Column(14).Width = 6;
                worksheet.Column(15).Width = 6;
                worksheet.Column(16).Width = 11;
                worksheet.Column(17).Width = 11;

                var data = dataTable.AsEnumerable()
                .Select(row => new
                {
                    Cuenta = row["Cod_CtaCont"].ToString(),
                    Descripcion = row["Des_CtaCont"].ToString(),
                    Tipo = row["Tip_Cuenta"].ToString(),
                    Nivel = row["Nivel_Saldo"].ToString(),
                    Anexo = row["Tipo_Anexo"].ToString(),
                    Anexoref = row["Anexo_ref"].ToString(),
                    Flg_AceptaCentro_Costo = row["Flg_AceptaCentro_Costo"].ToString(),
                    DocRef = row["Documento_Referencia"].ToString(),
                    DocRef2 = row["Documento_Referencia2"].ToString(),
                    Fecha_vencimiento = row["Fecha_vencimiento"].ToString(),
                    Area = row["Area"].ToString(),
                    Reg_CTA = row["Reg_CTA"].ToString(),
                    Moneda = row["Moneda"].ToString(),
                    ConcilBanco = row["Concil_Banco"].ToString(),
                    Mediopago = row["Medio_Pago"].ToString(),
                    Cuenta_Cargo = row["Cuenta_Cargo"].ToString(),
                    Cuenta_Abono = row["Cuenta_Abono"].ToString(),
                    Balance_General = row["Balance_General"].ToString(),
                    Gan_Perdi_por_funcion = row["Gan_Perdi_por_funcion"].ToString(),
                    Gan_Perdi_por_naturaleza = row["Gan_Perdi_por_naturaleza"].ToString(),
                    Centro_Costo = row["Centro_Costo"].ToString(), 

                    Ingresosalt = row["Ingresos_Gastos_Alt"].ToString(),
                    costoalt = row["Costos_Alt"].ToString(),
                    balancealt = row["Balance_General_Alt"].ToString(),
                    ganfunalt = row["Gan_Perdi_por_funcion_Alt"].ToString(),
                    gannatualt = row["Gan_Perdi_por_naturaleza_Alt"].ToString(),
 

                });
 

                worksheet.Cell(8, 1).InsertData(data);
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "PlanContable.xlsx");
                }



            }
        }

        


    }


    public class ProcedureRequest
    {
        public string Nombre { get; set; } = "";
        public Dictionary<string, string>? Parametros { get; set; }
    }
}



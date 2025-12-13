using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para usar ToListAsync()
using TextilesJocApi.Data;      // Para ver tu ApplicationDbContext
using TextilesJocApi.Models;    // Para ver tu modelo SegEmpresasW

namespace TextilesJocApi.Controllers
{
    // Esta etiqueta define la URL: api/SegEmpresas
    [Route("api/[controller]")]
    [ApiController]
    public class SegUsuariosController : ControllerBase
    {
        // 1. Declaramos una variable para guardar la conexión
        private readonly ApplicationDbContext _context;

        // 2. Constructor: Aquí recibimos la conexión abierta desde Program.cs
        public SegUsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 3. El Método GET (Obtener Datos)
        // Se activa cuando alguien entra a: https://localhost:xxxx/api/SegEmpresas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegEmpresasW>>> GetEmpresas()
        {
            // AQUI OCURRE LA MAGIA:
            // _context.SegEmpresasW -> Apunta a la tabla
            // .ToListAsync() -> Ejecuta el "SELECT * FROM SEG_EMPRESAS_W" en SQL
            var listaEmpresas = await _context.SegEmpresasW.ToListAsync();

            // Retorna los datos en formato JSON al navegador/React
            return Ok(listaEmpresas);
        }

        // 4. (Opcional) Método para buscar UNA sola empresa por su Código
        // Se activa con: api/SegEmpresas/01
        [HttpGet("{codigo}")]
        public async Task<ActionResult<SegEmpresasW>> GetEmpresaPorCodigo(string codigo)
        {
            var empresa = await _context.SegEmpresasW.FindAsync(codigo);

            if (empresa == null)
            {
                return NotFound(); // Retorna error 404 si no existe
            }

            return empresa;
        }
    }
}
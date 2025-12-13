using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TextilesJocApi.Data;
using TextilesJocApi.Models;

namespace TextilesJocApi.Controllers
{
    [Route("api/[controller]")] // La URL será: https://localhost:puerto/api/usuarios
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 1. CONSTRUCTOR: Aquí "inyectamos" la conexión a la BD para poder usarla
        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 2. MÉTODO GET: Para OBTENER todos los usuarios
        // Se llama visitando: GET api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Va a la base de datos, convierte la tabla a una lista y la devuelve
            return await _context.Usuarios.ToListAsync();
        }

        // 3. MÉTODO POST: Para CREAR un nuevo usuario
        // Se llama enviando datos a: POST api/usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Agrega el usuario que llega desde React (o Swagger) a la memoria
            _context.Usuarios.Add(usuario);

            // Guarda los cambios en la base de datos real (SQL Server)
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuarios", new { id = usuario.Id }, usuario);
        }
    }
}
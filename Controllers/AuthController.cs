using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // Para SQL
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TextilesJocApi.Controllers; // Asegúrate de usar tu namespace
using TextilesJocApi.Models; // <--- ¡AGREGA ESTO!

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly string _cadenaSQL;

    public AuthController(IConfiguration config)
    {
        _config = config;
        _cadenaSQL = _config.GetConnectionString("CadenaSQL");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO login)
    {
        // 1. VALIDAR CREDENCIALES (Usuario y Password)
        // Aquí deberías verificar la tabla USUARIOS_WEB_CONCAR.
        // Por simplicidad, asumiremos que validas que la clave coincida.
        // (En producción, usa contraseñas encriptadas/Hash).

        bool esValido = ValidarCredencialesEnBD(login.Usuario, login.Clave);

        if (!esValido)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });
        }

        // 2. OBTENER EL MENÚ COMPLETO (ARBOL)
        var menuJerarquico = ObtenerMenuArbol(login.Usuario);

        // 3. GENERAR TOKEN JWT
        var token = GenerarToken(login.Usuario);

        // 4. RETORNAR TODO AL FRONTEND
        return Ok(new
        {
            mensaje = "Login exitoso",
            token = token,
            usuario = login.Usuario,
            menu = menuJerarquico
        });
    }

    // --- MÉTODOS PRIVADOS (Lógica interna) ---

    private List<CardMenuDTO> ObtenerMenuArbol(string usuario)
    {
        var listaPlana = new List<MenuFlatItem>();

        // A. CONEXIÓN A SQL (Traemos la tabla plana)
        using (var conexion = new SqlConnection(_cadenaSQL))
        {
            conexion.Open();
            // Usamos el SP nuevo que trae todas las columnas (Card, Padre, Modulo, Form)
            using (var cmd = new SqlCommand("sp_LoginObtenerMenuArbol", conexion))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CodUsuario", usuario);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listaPlana.Add(new MenuFlatItem
                        {
                            CardID = reader["CardID"].ToString(),
                            CardTitulo = reader["CardTitulo"].ToString(),
                            PadreID = reader["PadreID"].ToString(),
                            PadreTitulo = reader["PadreTitulo"].ToString(),
                            ModuloID = reader["ModuloID"].ToString(),
                            ModuloTitulo = reader["ModuloTitulo"].ToString(),
                            FormID = reader["FormID"].ToString(),
                            FormTitulo = reader["FormTitulo"].ToString()
                        });
                    }
                }
            }
        }

        // B. TRANSFORMACIÓN LINQ (Plana -> Jerárquica)
        var menuArbol = listaPlana
            .GroupBy(x => new { x.CardID, x.CardTitulo })
            .Select(gCard => new CardMenuDTO
            {
                Key = gCard.Key.CardID,
                Title = gCard.Key.CardTitulo,
                Padres = gCard
                    .GroupBy(p => new { p.PadreID, p.PadreTitulo })
                    .Select(gPadre => new PadreMenuDTO
                    {
                        Key = gPadre.Key.PadreID,
                        Title = gPadre.Key.PadreTitulo,
                        Modulos = gPadre
                            .GroupBy(m => new { m.ModuloID, m.ModuloTitulo })
                            .Select(gMod => new ModuloMenuDTO
                            {
                                Key = gMod.Key.ModuloID,
                                Title = gMod.Key.ModuloTitulo,
                                Items = gMod
                                    .Select(f => new FormularioDTO
                                    {
                                        Key = f.FormID,
                                        Label = f.FormTitulo
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return menuArbol;
    }
    private bool ValidarCredencialesEnBD(string usuario, string clave)
    {
        // Aquí haz un simple SELECT count(*) para ver si existe user/pass y está activo
        using (var conexion = new SqlConnection(_cadenaSQL))
        {
            conexion.Open();
            string query = "SELECT COUNT(*) FROM USUARIOS_WEB_CONCAR WHERE cod_usuario = @u AND password = @p AND estado = 'ACTIVO'";
            using (var cmd = new SqlCommand(query, conexion))
            {
                cmd.Parameters.AddWithValue("@u", usuario);
                cmd.Parameters.AddWithValue("@p", clave);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }

    private string GenerarToken(string usuario)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, usuario) };

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
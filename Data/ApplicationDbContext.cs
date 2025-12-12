using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TEXTILJOC_ConcarWeb.Models;

namespace TEXTILJOC_ConcarWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor obligatorio: recibe la configuración de conexión
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aquí declaramos nuestras tablas.
        // El nombre "Usuarios" será el nombre real de la tabla en SQL Server.
        public DbSet<Usuario> Usuarios { get; set; }

        // Declaramos la tabla nueva
        public DbSet<SegUsuarioW> SgMuestraUsuariosW { get; set; }
        public DbSet<SegEmpresasW> SegEmpresasW { get; set; }
    }
}
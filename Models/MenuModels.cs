using System.Collections.Generic;

namespace TextilesJocApi.Models // IMPORTANTE: Cambia 'TuProyecto' por el nombre real de tu proyecto
{
    // ==========================================
    // 1. CLASE PLANA (Para recibir datos de SQL)
    // ==========================================
    public class MenuFlatItem
    {
        // Nivel 1: Card
        public string CardID { get; set; }
        public string CardTitulo { get; set; }

        // Nivel 2: Padre
        public string PadreID { get; set; }
        public string PadreTitulo { get; set; }

        // Nivel 3: Módulo
        public string ModuloID { get; set; }
        public string ModuloTitulo { get; set; }

        // Nivel 4: Formulario
        public string FormID { get; set; }
        public string FormTitulo { get; set; }
    }

    // ==========================================
    // 2. CLASES JERÁRQUICAS (Para enviar al Front)
    // ==========================================

    // Nivel 1
    public class CardMenuDTO
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<PadreMenuDTO> Padres { get; set; } = new List<PadreMenuDTO>();
    }

    // Nivel 2
    public class PadreMenuDTO
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<ModuloMenuDTO> Modulos { get; set; } = new List<ModuloMenuDTO>();
    }

    // Nivel 3
    public class ModuloMenuDTO
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<FormularioDTO> Items { get; set; } = new List<FormularioDTO>();
    }

    // Nivel 4
    public class FormularioDTO
    {
        public string Key { get; set; }
        public string Label { get; set; }
        public string Ruta { get; set; } // Opcional
    }
}
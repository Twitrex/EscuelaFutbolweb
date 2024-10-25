using System.ComponentModel.DataAnnotations;

namespace EscuelaFutbolweb.Models
{
    public class Equipo
    {
        public int EquipoID { get; set; }  // ID del equipo

        [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreEquipo { get; set; }  // Nombre del equipo

        public int? CategoriaID { get; set; }  // ID de la categoría (puede ser NULL)
        public int? EntrenadorID { get; set; }  // ID del entrenador (puede ser NULL)

        // Campos solo para mostrar
        public string? Categoria { get; set; }  // Nombre de la categoría (solo para mostrar, no en la tabla)
        public string? Entrenador { get; set; }  // Nombre del entrenador (solo para mostrar, no en la tabla)

        public bool Activo { get; set; }  // Estado del equipo (Activo/Inactivo)
    }
}


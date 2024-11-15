using System.ComponentModel.DataAnnotations;
namespace EscuelaFutbolweb.Models
{
    public class Alumno
    {
        [Key]
        [Display(Name = "ID Alumno")]
        public int AlumnoID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required]
        [StringLength(8, ErrorMessage = "El DNI no puede tener más de 8 caracteres.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener solo números y tener exactamente 8 dígitos.")]
        [Display(Name = "DNI")]
        public string DNI { get; set; }

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Display(Name = "Edad")]
        public int? Edad { get; set; }

        [Display(Name = "Categoría")]
        public string? Categoria { get; set; }

        [Display(Name = "Categoría ID")]
        public int? CategoriaID { get; set; }

        [Display(Name = "Puesto")]
        public string? Puesto { get; set; }

        [Display(Name = "Puesto ID")]
        public int? PuestoID { get; set; }

        [StringLength(15)]
        [Display(Name = "Teléfono")]
        [DataType(DataType.PhoneNumber)]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El campo de Email es necesario")]
        [StringLength(255)]
        [Display(Name = "Correo Electrónico")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }
        public string? SubCategoria { get; set; }
    }
}
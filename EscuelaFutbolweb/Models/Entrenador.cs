namespace EscuelaFutbolweb.Models
{
    public class Entrenador
    {
        public int EntrenadorID { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; }
    }
}

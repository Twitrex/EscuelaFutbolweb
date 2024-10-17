using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using EscuelaFutbolweb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EscuelaFutbolweb.Controllers
{
    public class EquipoController : Controller
    {
        private readonly IConfiguration _config;
        public EquipoController(IConfiguration config)
        {
            _config = config;
        }

        // Método para listar equipos
        public List<Equipo> ListarEquipos()
        {
            List<Equipo> lista = new List<Equipo>();
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spListarEquipos", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Equipo equipo = new Equipo
                    {
                        EquipoID = dr.GetInt32(0),
                        NombreEquipo = dr.GetString(1),
                        Categoria = dr.IsDBNull(2) ? "Sin Categoría" : dr.GetString(2),
                        Entrenador = dr.IsDBNull(3) ? "Sin Entrenador" : dr.GetString(3),
                        Activo = dr.GetBoolean(4)
                    };
                    lista.Add(equipo);
                }
                dr.Close();
            }
            return lista;
        }

        // Método para mostrar la lista de equipos
        public async Task<IActionResult> Equipos()
        {
            return View(await Task.Run(() => ListarEquipos()));
        }

        // Método para obtener las categorías y entrenadores en los combo box
        private void CargarDatosEquipo()
        {
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();

                // Cargar categorías
                SqlCommand cmdCategorias = new SqlCommand("spCargarCategorias", cnn);
                SqlDataReader drCategorias = cmdCategorias.ExecuteReader();
                List<SelectListItem> categorias = new List<SelectListItem>();
                //Para agregar un item desde el método
                //categorias.Add(new SelectListItem { Value = "", Text = "Seleccione una categoría" });
                while (drCategorias.Read())
                {
                    categorias.Add(new SelectListItem { Value = drCategorias["CategoriaID"].ToString(), Text = drCategorias["NombreCategoria"].ToString() });
                }
                ViewBag.Categorias = categorias;
                drCategorias.Close();

                // Cargar entrenadores
                SqlCommand cmdEntrenadores = new SqlCommand("spCargarEntrenadores", cnn);
                SqlDataReader drEntrenadores = cmdEntrenadores.ExecuteReader();
                List<SelectListItem> entrenadores = new List<SelectListItem>();
                //Para agregar un item desde el método
                //entrenadores.Add(new SelectListItem { Value = "", Text = "Seleccione un entrenador" });
                while (drEntrenadores.Read())
                {
                    entrenadores.Add(new SelectListItem { Value = drEntrenadores["EntrenadorID"].ToString(), Text = drEntrenadores["Nombre"].ToString() });
                }
                ViewBag.Entrenadores = entrenadores;
                drEntrenadores.Close();
            }
        }

        // Método GET para crear un nuevo equipo
        public async Task<IActionResult> NuevoEquipo()
        {
            // Cargar categorías y entrenadores en los dropdowns
            CargarDatosEquipo();
            return View(await Task.Run(() => new Equipo()));
        }

        // Método POST para agregar un nuevo equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoEquipo(Equipo equipo)
        {
            if (!ModelState.IsValid)
            {
                CargarDatosEquipo();
                return View(equipo);
            }
            equipo.Activo = true;
            CrearEquipo(equipo);
            return RedirectToAction(nameof(Equipos));
        }

        // Método para crear un nuevo equipo en la base de datos
        public void CrearEquipo(Equipo equipo)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand("spCrearEquipo", cnn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Asignar los parámetros para el stored procedure
                    cmd.Parameters.AddWithValue("@NombreEquipo", equipo.NombreEquipo);
                    cmd.Parameters.AddWithValue("@CategoriaID", equipo.CategoriaID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntrenadorID", equipo.EntrenadorID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", equipo.Activo);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear el equipo: " + ex.Message);
            }
        }


        // Método GET para editar un equipo
        public async Task<IActionResult> EditarEquipo(int id)
        {
            Equipo equipo = SeleccionarEquipoPorID(id);
            if (equipo == null)
            {
                return NotFound();
            }

            // Cargar categorías y entrenadores en los dropdowns
            CargarDatosEquipo();
            return View(await Task.Run(() => equipo));
        }

        // Método POST para actualizar un equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEquipo(Equipo equipo)
        {
            if (!ModelState.IsValid)
            {
                // Si hay errores en el formulario, recargar las categorías y entrenadores
                CargarDatosEquipo();
                return View(equipo);
            }

            ActualizarEquipo(equipo);
            Console.WriteLine("Equipo creado correctamente. Redirigiendo a la lista de equipos.");
            return RedirectToAction(nameof(Equipos));
        }

        // Método para seleccionar un equipo por ID
        public Equipo SeleccionarEquipoPorID(int equipoID)
        {
            Equipo equipo = new Equipo();
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spSeleccionarEquipo", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoID);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    equipo.EquipoID = dr.GetInt32(0);
                    equipo.NombreEquipo = dr.GetString(1);
                    equipo.CategoriaID = dr.IsDBNull(2) ? (int?)null : dr.GetInt32(2);
                    equipo.EntrenadorID = dr.IsDBNull(3) ? (int?)null : dr.GetInt32(3);
                    equipo.Activo = dr.GetBoolean(4);
                }
                dr.Close();
            }
            return equipo;
        }

        // Método para actualizar un equipo
        public void ActualizarEquipo(Equipo equipo)
        {
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spActualizarEquipo", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipo.EquipoID);
                cmd.Parameters.AddWithValue("@NombreEquipo", equipo.NombreEquipo);
                cmd.Parameters.AddWithValue("@CategoriaID", equipo.CategoriaID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@EntrenadorID", equipo.EntrenadorID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", equipo.Activo);
                cmd.ExecuteNonQuery();
            }
        }

        // Método para asignar un alumno a un equipo
        public void AsignarAlumnoAEquipo(int equipoID, int alumnoID)
        {
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spAsignarAlumnoAEquipo", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoID);
                cmd.Parameters.AddWithValue("@AlumnoID", alumnoID);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

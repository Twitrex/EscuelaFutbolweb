using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using EscuelaFutbolweb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace EscuelaFutbolweb.Controllers
{
    public class EquipoController : Controller
    {
        private readonly IConfiguration _config;
        public EquipoController(IConfiguration config)
        {
            _config = config;
        }

        // Listar todos los equipos
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
                        Categoria = dr.GetString(2),
                        Entrenador = dr.GetString(3),
                        Activo = dr.GetBoolean(4)
                    };
                    lista.Add(equipo);
                }
                dr.Close();
            }
            return lista;
        }

        // Vista principal para listar los equipos
        [Authorize(Roles = "Administrador, Entrenador")]
        public async Task<IActionResult> Equipos()
        {
            return View(await Task.Run(() => ListarEquipos()));
        }

        public List<Equipo> ObtenerEquiposInactivos()
        {
            List<Equipo> equiposInactivos = new List<Equipo>();

            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spListarEquiposInactivos", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        equiposInactivos.Add(new Equipo
                        {
                            EquipoID = dr.GetInt32(0),
                            NombreEquipo = dr.GetString(1),
                            Categoria = dr.IsDBNull(2) ? "Sin Categoría" : dr.GetString(2),
                            Entrenador = dr.IsDBNull(3) ? "Sin Entrenador" : dr.GetString(3),
                            Activo = dr.GetBoolean(4)
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al listar los equipos inactivos: " + ex.Message);
            }

            return equiposInactivos;
        }

        public IActionResult EquiposInactivos()
        {
            var equiposInactivos = ObtenerEquiposInactivos();
            return View(equiposInactivos);  // Retornar la vista con los equipos inactivos
        }

        public void ActivarEquipoEnBD(int equipoID)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spActivarEquipo", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EquipoID", equipoID);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al activar el equipo: " + ex.Message);
            }
        }

        public IActionResult ActivarEquipo(int id)
        {
            ActivarEquipoEnBD(id);
            return RedirectToAction(nameof(EquiposInactivos));  // Volver a la lista de equipos inactivos
        }

        // Obtener categorías para el dropdown
        public List<SelectListItem> ObtenerCategorias()
        {
            List<SelectListItem> categorias = new List<SelectListItem>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spCargarCategorias", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    categorias.Add(new SelectListItem
                    {
                        Value = dr["CategoriaID"].ToString(),
                        Text = dr["NombreCategoria"].ToString()
                    });
                }
                dr.Close();
            }
            return categorias;
        }

        // Obtener entrenadores para el dropdown
        public List<SelectListItem> ObtenerEntrenadores()
        {
            List<SelectListItem> entrenadores = new List<SelectListItem>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spCargarEntrenadores", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    entrenadores.Add(new SelectListItem
                    {
                        Value = dr["EntrenadorID"].ToString(),
                        Text = dr["Nombre"].ToString()
                    });
                }
                dr.Close();
            }
            return entrenadores;
        }

        // Mostrar el formulario para agregar un nuevo equipo
        public IActionResult NuevoEquipo()
        {
            ViewBag.Categorias = ObtenerCategorias();
            ViewBag.Entrenadores = ObtenerEntrenadores();
            return View(new Equipo());
        }

        public void InsertarEquipo(Equipo nuevoEquipo)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spAgregarEquipo", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NombreEquipo", nuevoEquipo.NombreEquipo);
                    cmd.Parameters.AddWithValue("@CategoriaID", nuevoEquipo.CategoriaID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntrenadorID", nuevoEquipo.EntrenadorID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", nuevoEquipo.Activo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar el equipo: " + ex.Message);
            }
        }


        // Método POST para agregar un nuevo equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoEquipo(Equipo nuevoEquipo)
        {
            /*if (!ModelState.IsValid)
            {
                ViewBag.Categorias = ObtenerCategorias();
                ViewBag.Entrenadores = ObtenerEntrenadores();
                return View(nuevoEquipo);
            }*/
            if (!ModelState.IsValid)
            {
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                ViewBag.Categorias = ObtenerCategorias();
                ViewBag.Entrenadores = ObtenerEntrenadores();
                return View(nuevoEquipo);
            }


            InsertarEquipo(nuevoEquipo);

            return RedirectToAction(nameof(Equipos));
        }


        // Mostrar el formulario para editar un equipo existente
        public IActionResult EditarEquipo(int id)
        {
            Equipo equipo = SeleccionarEquipo(id);
            if (equipo == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = ObtenerCategorias();
            ViewBag.Entrenadores = ObtenerEntrenadores();
            return View(equipo);
        }

        // Seleccionar equipo por ID
        public Equipo SeleccionarEquipo(int equipoID)
        {
            Equipo equipo = null;
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spSeleccionarEquipo", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoID);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    equipo = new Equipo
                    {
                        EquipoID = dr.GetInt32(0),
                        NombreEquipo = dr.GetString(1),
                        CategoriaID = dr.IsDBNull(2) ? (int?)null : dr.GetInt32(2),
                        EntrenadorID = dr.IsDBNull(3) ? (int?)null : dr.GetInt32(3),
                        Activo = dr.GetBoolean(4)
                    };
                }
                dr.Close();
            }
            return equipo;
        }

        // Método POST para actualizar un equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEquipo(Equipo equipo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = ObtenerCategorias();
                ViewBag.Entrenadores = ObtenerEntrenadores();
                return View(equipo);
            }

            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spActualizarEquipo", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EquipoID", equipo.EquipoID);
                    cmd.Parameters.AddWithValue("@NombreEquipo", equipo.NombreEquipo);
                    cmd.Parameters.AddWithValue("@CategoriaID", equipo.CategoriaID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EntrenadorID", equipo.EntrenadorID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", equipo.Activo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar el equipo: " + ex.Message);
            }

            return RedirectToAction(nameof(Equipos));
        }

        // Método para eliminar un equipo
        public IActionResult EliminarEquipo(int id)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spEliminarEquipo", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EquipoID", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar el equipo: " + ex.Message);
            }

            return RedirectToAction(nameof(Equipos));
        }

        // GET: Mostrar vista para asignar alumnos
        [HttpGet]
        public IActionResult AsignarAlumnos(int equipoId)
        {
            ViewBag.EquipoID = equipoId;
            ViewBag.AlumnosDisponibles = ObtenerAlumnosDisponibles(equipoId); // Alumnos no asignados
            ViewBag.NombreEquipo = ObtenerNombreEquipo(equipoId); // Obtener el nombre del equipo
            return View();
        }

        // POST: Asignar alumnos al equipo
        [HttpPost]
        public IActionResult AsignarAlumnos(int equipoId, List<int> alumnoIds)
        {
            foreach (var alumnoId in alumnoIds)
            {
                // Verificar si el alumno ya está asignado al equipo
                if (!EsAlumnoAsignadoAlEquipo(equipoId, alumnoId))
                {
                    AsignarAlumnoAEquipo(equipoId, alumnoId);
                }
                else
                {
                    // Mensaje o acción opcional si el alumno ya está asignado al equipo
                    Console.WriteLine($"El alumno con ID {alumnoId} ya está asignado al equipo con ID {equipoId}.");
                }
            }
            return RedirectToAction("Detalles", new { id = equipoId });
        }

        // Método para verificar si el alumno ya está asignado al equipo
        private bool EsAlumnoAsignadoAlEquipo(int equipoId, int alumnoId)
        {
            bool yaAsignado = false;
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spVerificarAlumnoEnEquipo", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                cmd.Parameters.AddWithValue("@AlumnoID", alumnoId);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    // Convertir el valor devuelto (0 o 1) en un booleano
                    yaAsignado = dr.GetInt32(0) == 1;
                }
                dr.Close();
            }
            return yaAsignado;
        }

        // Método para obtener alumnos disponibles
        private List<SelectListItem> ObtenerAlumnosDisponibles(int equipoId)
        {
            List<SelectListItem> alumnos = new List<SelectListItem>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spObtenerAlumnosNoAsignados", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    alumnos.Add(new SelectListItem
                    {
                        Value = dr["AlumnoID"].ToString(),
                        Text = dr["NombreCompleto"].ToString()
                    });
                }
                dr.Close();
            }
            return alumnos;
        }

        // Método para obtener el nombre del equipo
        private string ObtenerNombreEquipo(int equipoId)
        {
            string nombreEquipo = "";
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spObtenerNombreEquipo", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    nombreEquipo = dr["NombreEquipo"].ToString();
                }
                dr.Close();
            }
            return nombreEquipo;
        }

        // Método para asignar alumno al equipo
        private void AsignarAlumnoAEquipo(int equipoId, int alumnoId)
        {
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spAsignarAlumnoAEquipo", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                cmd.Parameters.AddWithValue("@AlumnoID", alumnoId);
                cmd.ExecuteNonQuery();
            }
        }

        // Método para obtener los alumnos asignados a un equipo
        public List<Alumno> ObtenerAlumnosPorEquipo(int equipoId)
        {
            List<Alumno> alumnos = new List<Alumno>();
            using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("spObtenerAlumnosPorEquipo", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    alumnos.Add(new Alumno
                    {
                        AlumnoID = dr.GetInt32(0),
                        Nombre = dr.GetString(1),
                        Apellido = dr.GetString(2),
                        FechaNacimiento = dr.GetDateTime(3),
                        Categoria = dr.IsDBNull(4) ? null : dr.GetString(4),
                        Puesto = dr.IsDBNull(5) ? null : dr.GetString(5)
                    });
                }
                dr.Close();
            }
            return alumnos;
        }

        // Acción para la vista que muestra los alumnos de un equipo
        public IActionResult VerAlumnos(int equipoId)
        {
            var alumnos = ObtenerAlumnosPorEquipo(equipoId);
            ViewBag.EquipoID = equipoId;
            ViewBag.NombreEquipo = ObtenerNombreEquipo(equipoId);
            ViewBag.MensajeExito = TempData["MensajeExito"]?.ToString(); // Mensaje de éxito si existe
            return View(alumnos);
        }

        

        public IActionResult AsignarMultiplesAlumnos(int equipoId)
        {
            ViewBag.EquipoID = equipoId;
            ViewBag.NombreEquipo = ObtenerNombreEquipo(equipoId);
            ViewBag.AlumnosDisponibles = ObtenerAlumnosDisponibles(equipoId);
            return View();
        }

        // POST: Asignar múltiples alumnos a un equipo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarMultiplesAlumnos(int equipoId, List<int> alumnoIds)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EquipoID = equipoId;
                ViewBag.NombreEquipo = ObtenerNombreEquipo(equipoId);
                ViewBag.AlumnosDisponibles = ObtenerAlumnosDisponibles(equipoId);
                return View(alumnoIds);
            }

            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    foreach (var alumnoId in alumnoIds)
                    {
                        SqlCommand cmd = new SqlCommand("spAsignarAlumnoAEquipo", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                        cmd.Parameters.AddWithValue("@AlumnoID", alumnoId);
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["MensajeExito"] = "Alumnos asignados con éxito.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al asignar alumnos: " + ex.Message);
                ViewBag.EquipoID = equipoId;
                ViewBag.NombreEquipo = ObtenerNombreEquipo(equipoId);
                ViewBag.AlumnosDisponibles = ObtenerAlumnosDisponibles(equipoId);
                return View(alumnoIds);
            }

            return RedirectToAction("VerAlumnos", new { equipoId });
        }

        // Método para buscar alumnos por nombre o apellido
        [HttpGet]
        public IActionResult BuscarAlumnos(string nombre)
        {
            List<Alumno> alumnos = new List<Alumno>();
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spBuscarAlumnosPorNombre", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        alumnos.Add(new Alumno
                        {
                            AlumnoID = dr.GetInt32(0),
                            Nombre = dr.GetString(1),
                            Apellido = dr.GetString(2)
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al buscar alumnos: " + ex.Message);
                return StatusCode(500, "Error interno del servidor");
            }

            return Json(alumnos);
        }
    }
}

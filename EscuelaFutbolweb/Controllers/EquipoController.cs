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

        // GET: Mostrar vista para asignar alumnos a un equipo específico
        [HttpGet]
        public IActionResult AsignarAlumnos(int equipoId)
        {
            ViewBag.EquipoID = equipoId;
            ViewBag.AlumnosDisponibles = ObtenerAlumnosDisponibles(equipoId);
            return View();
        }

        // POST: Asignar alumnos al equipo
        [HttpPost]
        public IActionResult AsignarAlumnos(int equipoId, List<int> alumnoIds)
        {
            foreach (var alumnoId in alumnoIds)
            {
                AsignarAlumnoAEquipo(equipoId, alumnoId);
            }

            return RedirectToAction("Detalles", new { id = equipoId });
        }

        // Método para obtener la lista de alumnos que no están asignados a un equipo
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

        // Método para asignar un alumno a un equipo
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
    }
}

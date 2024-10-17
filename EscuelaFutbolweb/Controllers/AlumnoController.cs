﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using EscuelaFutbolweb.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace EscuelaFutbolweb.Controllers
{
    public class AlumnoController : Controller
    {
        private readonly IConfiguration _config;
        public AlumnoController(IConfiguration config)
        {
            _config = config;
        }

        public List<Alumno> ListarAlumnos(string nombre = null, string dni = null, int? categoriaID = null, int? puestoID = null, bool? estado = null)
        {
            List<Alumno> lista = new List<Alumno>();
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spListarAlumnosFiltrados", cnn);
                cmd.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros de filtro
                cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(nombre) ? (object)DBNull.Value : nombre);
                cmd.Parameters.AddWithValue("@DNI", string.IsNullOrEmpty(dni) ? (object)DBNull.Value : dni);
                cmd.Parameters.AddWithValue("@CategoriaID", categoriaID.HasValue ? (object)categoriaID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@PuestoID", puestoID.HasValue ? (object)puestoID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", estado.HasValue ? (object)estado.Value : DBNull.Value);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Alumno alumno = new Alumno
                    {
                        AlumnoID = dr.GetInt32(0),
                        Nombre = dr.GetString(1),
                        Apellido = dr.GetString(2),
                        DNI = dr.GetString(3),
                        Edad = dr.GetInt32(4),
                        Categoria = dr.GetString(5),
                        Puesto = dr.GetString(6)
                    };
                    lista.Add(alumno);
                }
                dr.Close();
            }
            return lista;
        }

        public async Task<IActionResult> Alumnos(string nombre, string dni, int? categoriaID, int? puestoID)
        {
            // Cargar categorías y puestos para los dropdowns
            ViewBag.Categorias = ObtenerCategorias();
            ViewBag.Puestos = ObtenerPuestos();

            // Llama al método que lista los alumnos con los filtros aplicados
            return View(await Task.Run(() => ListarAlumnos(nombre, dni, categoriaID, puestoID, true)));  // Estado = Activo
        }

        public void agregarAlumno(Alumno nuevoAlumno)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spAgregarAlumno", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", nuevoAlumno.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", nuevoAlumno.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", nuevoAlumno.DNI);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", nuevoAlumno.FechaNacimiento);
                    cmd.Parameters.AddWithValue("@CategoriaID", nuevoAlumno.CategoriaID);
                    cmd.Parameters.AddWithValue("@PuestoID", nuevoAlumno.PuestoID);
                    cmd.Parameters.AddWithValue("@Telefono", nuevoAlumno.Telefono);
                    cmd.Parameters.AddWithValue("@Email", nuevoAlumno.Email);
                    cmd.Parameters.AddWithValue("@Activo", nuevoAlumno.Activo);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar el alumno: " + ex.Message);
            }
        }


        public async Task<IActionResult> NuevoAlumno()
        {
            // Cargar las listas de categorías y puestos para el formulario
            ViewBag.Categorias = new SelectList(ObtenerCategorias(), "Value", "Text");
            ViewBag.Puestos = new SelectList(ObtenerPuestos(), "Value", "Text");

            return View();
        }

        public void AsignarCategoria(Alumno nuevoAlumno)
        {
            // Calcular la edad del alumno
            var edad = DateTime.Now.Year - nuevoAlumno.FechaNacimiento.Year;
            if (nuevoAlumno.FechaNacimiento > DateTime.Now.AddYears(-edad))
            {
                edad--;
            }

            // Asignar la categoría según la edad
            if (edad >= 5 && edad <= 7)
            {
                nuevoAlumno.CategoriaID = 1; // Prebenjamines
            }
            else if (edad >= 8 && edad <= 9)
            {
                nuevoAlumno.CategoriaID = 2; // Benjamines
            }
            else if (edad >= 10 && edad <= 11)
            {
                nuevoAlumno.CategoriaID = 3; // Alevines
            }
            else if (edad >= 12 && edad <= 13)
            {
                nuevoAlumno.CategoriaID = 4; // Infantiles
            }
            else if (edad >= 14 && edad <= 15)
            {
                nuevoAlumno.CategoriaID = 5; // Cadetes
            }
            else if (edad >= 16 && edad <= 18)
            {
                nuevoAlumno.CategoriaID = 6; // Juveniles
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoAlumno(Alumno nuevoAlumno)
        {
            if (!ModelState.IsValid)
            {
                // Volver a cargar los puestos si hay un error de validación
                ViewBag.Puestos = new SelectList(ObtenerPuestos(), "Value", "Text", nuevoAlumno.PuestoID);
                return View(nuevoAlumno);
            }

            // Asignar un valor por defecto a Activo
            nuevoAlumno.Activo = true;  // O false, según lo que prefieras

            // Si PuestoID no fue seleccionado, asignar NULL
            if (nuevoAlumno.PuestoID == 0)
            {
                nuevoAlumno.PuestoID = null;
            }

            // Asignar la categoría automáticamente según la edad del alumno
            AsignarCategoria(nuevoAlumno);

            // Llamar al método para insertar un nuevo alumno en la base de datos
            agregarAlumno(nuevoAlumno);

            return RedirectToAction(nameof(Alumnos));  // Redirigir a la lista de alumnos después de agregar
        }

        public Alumno seleccionarAlumnoPorID(int idAlumno)
        {
            Alumno alumno = new Alumno();
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spSeleccionarAlumno", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlumnoID", idAlumno);

                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        alumno.AlumnoID = dr.GetInt32(0);
                        alumno.Nombre = dr.GetString(1);
                        alumno.Apellido = dr.GetString(2);
                        alumno.DNI = dr.GetString(3);
                        alumno.FechaNacimiento = dr.GetDateTime(4);

                        // Manejar valores nulos para CategoriaID y PuestoID
                        alumno.CategoriaID = dr.IsDBNull(5) ? (int?)null : dr.GetInt32(5);
                        alumno.PuestoID = dr.IsDBNull(6) ? (int?)null : dr.GetInt32(6);

                        alumno.Telefono = dr.IsDBNull(7) ? null : dr.GetString(7);
                        alumno.Email = dr.IsDBNull(8) ? null : dr.GetString(8);
                        alumno.Activo = dr.GetBoolean(9);  // Checkbox Activo
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al seleccionar el alumno: " + ex.Message);
            }
            return alumno;
        }

        public Alumno obtenerDetalleAlumnoPorID(int idAlumno)
        {
            Alumno alumno = new Alumno();
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spObtenerDetalleAlumno", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlumnoID", idAlumno);

                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        alumno.AlumnoID = dr.GetInt32(0);
                        alumno.Nombre = dr.GetString(1);
                        alumno.Apellido = dr.GetString(2);
                        alumno.DNI = dr.GetString(3);
                        alumno.FechaNacimiento = dr.GetDateTime(4);
                        alumno.Categoria = dr.IsDBNull(5) ? null : dr.GetString(5);  // Obtener NombreCategoria
                        alumno.Puesto = dr.IsDBNull(6) ? null : dr.GetString(6);      // Obtener NombrePuesto
                        alumno.Telefono = dr.IsDBNull(7) ? null : dr.GetString(7);
                        alumno.Email = dr.IsDBNull(8) ? null : dr.GetString(8);
                        alumno.Activo = dr.GetBoolean(9);
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener detalle del alumno: " + ex.Message);
            }
            return alumno;
        }

        public async Task<IActionResult> DetalleAlumno(int id)
        {
            Alumno alumno = obtenerDetalleAlumnoPorID(id);  // Usamos el nuevo método con SP
            if (alumno == null)
            {
                return NotFound();
            }

            // Cargar la vista con el modelo del alumno
            return View(alumno);
        }


        public void actualizarAlumno(Alumno alumno)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spActualizarAlumno", cn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Enviar los parámetros al procedimiento almacenado
                    cmd.Parameters.AddWithValue("@AlumnoID", alumno.AlumnoID);
                    cmd.Parameters.AddWithValue("@Nombre", alumno.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", alumno.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", alumno.DNI);
                    cmd.Parameters.AddWithValue("@FechaNacimiento", alumno.FechaNacimiento);

                    // Si CategoriaID es null, enviar DBNull.Value
                    if (alumno.CategoriaID.HasValue)
                        cmd.Parameters.AddWithValue("@CategoriaID", alumno.CategoriaID.Value);
                    else
                        cmd.Parameters.AddWithValue("@CategoriaID", DBNull.Value);

                    // Si PuestoID es null, enviar DBNull.Value
                    if (alumno.PuestoID.HasValue)
                        cmd.Parameters.AddWithValue("@PuestoID", alumno.PuestoID.Value);
                    else
                        cmd.Parameters.AddWithValue("@PuestoID", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Telefono", alumno.Telefono ?? (object)DBNull.Value);  // Manejar null para Telefono
                    cmd.Parameters.AddWithValue("@Email", alumno.Email ?? (object)DBNull.Value);  // Manejar null para Email
                    cmd.Parameters.AddWithValue("@Activo", alumno.Activo);

                    cmd.ExecuteNonQuery();  // Ejecuta el procedimiento almacenado
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar el alumno: " + ex.Message);
            }
        }


        public List<SelectListItem> ObtenerCategorias()
        {
            List<SelectListItem> categorias = new List<SelectListItem>();
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener las categorías: " + ex.Message);
            }
            return categorias;
        }

        public List<SelectListItem> ObtenerPuestos()
        {
            List<SelectListItem> puestos = new List<SelectListItem>();
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spCargarPuestos", cn);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        puestos.Add(new SelectListItem
                        {
                            Value = dr["PuestoID"].ToString(),
                            Text = dr["NombrePuesto"].ToString()
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al obtener los puestos: " + ex.Message);
            }
            return puestos;
        }

        public async Task<IActionResult> EditarAlumno(int id)
        {
            // Obtener el alumno por ID
            Alumno alumno = seleccionarAlumnoPorID(id);
            if (alumno == null)
            {
                return RedirectToAction(nameof(Alumnos));  // Redirigir si el alumno no existe
            }

            // Cargar categorías y puestos para los combobox
            ViewBag.Categorias = new SelectList(ObtenerCategorias(), "Value", "Text", alumno.Categoria);
            ViewBag.Puestos = new SelectList(ObtenerPuestos(), "Value", "Text", alumno.Puesto);

            // Asegúrate de que el valor de Activo está siendo correctamente asignado
            Console.WriteLine(alumno.Activo);  // Verifica que el valor de Activo se está recibiendo

            return View(alumno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAlumno(Alumno alumno)
        {
            if (!ModelState.IsValid)
            {
                // Volver a cargar las categorías y puestos si hay un error de validación
                ViewBag.Categorias = new SelectList(ObtenerCategorias(), "Value", "Text", alumno.CategoriaID);
                ViewBag.Puestos = new SelectList(ObtenerPuestos(), "Value", "Text", alumno.PuestoID);
                return View(alumno);
            }

            // Si CategoriaID o PuestoID son null, mantenerlos como null
            if (alumno.CategoriaID == 0)
            {
                alumno.CategoriaID = null;
            }

            if (alumno.PuestoID == 0)
            {
                alumno.PuestoID = null;
            }

            // Ejecutar la actualización
            actualizarAlumno(alumno);

            return RedirectToAction(nameof(Alumnos));  // Redirigir a la lista de alumnos después de la actualización
        }

        public List<Alumno> ListarAlumnosInactivos()
        {
            List<Alumno> lista = new List<Alumno>();
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spListarAlumnosInactivos", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Alumno alumno = new Alumno
                    {
                        AlumnoID = dr.GetInt32(0),
                        Nombre = dr.GetString(1),
                        Apellido = dr.GetString(2),
                        DNI = dr.GetString(3),
                        FechaNacimiento = dr.GetDateTime(4),
                        Telefono = dr.IsDBNull(5) ? null : dr.GetString(5),
                        Email = dr.IsDBNull(6) ? null : dr.GetString(6),
                        Activo = dr.GetBoolean(7)
                    };
                    lista.Add(alumno);
                }
                dr.Close();
            }
            return lista;
        }

        public async Task<IActionResult> AlumnosInactivos()
        {
            return View(await Task.Run(() => ListarAlumnosInactivos()));
        }

        public void ActivarAlumno(int idAlumno)
        {
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spActivarAlumno", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AlumnoID", idAlumno);
                cmd.ExecuteNonQuery();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Activar(int id)
        {
            ActivarAlumno(id);
            return RedirectToAction(nameof(AlumnosInactivos));
        }

    }
}
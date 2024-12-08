using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using EscuelaFutbolweb.Models;
using Microsoft.AspNetCore.Authorization;

namespace EscuelaFutbolweb.Controllers
{
    public class EntrenadorController : Controller
    {
        private readonly IConfiguration _config;
        public EntrenadorController(IConfiguration config)
        {
            _config = config;
        }

        // Método para listar entrenadores
        public List<Entrenador> ListarEntrenadores()
        {
            List<Entrenador> lista = new List<Entrenador>();
            using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:sql"]))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("spListarEntrenadores", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Entrenador entrenador = new Entrenador
                    {
                        EntrenadorID = dr.GetInt32(0),
                        Nombre = dr.GetString(1),
                        Apellido = dr.GetString(2),
                        DNI = dr.GetString(3),
                        Telefono = dr.IsDBNull(4) ? null : dr.GetString(4),
                        Email = dr.IsDBNull(5) ? null : dr.GetString(5),
                        Activo = dr.GetBoolean(6)
                    };
                    lista.Add(entrenador);
                }
                dr.Close();
            }
            return lista;
        }

        // Método para mostrar la lista de entrenadores
        [Authorize(Roles = "Administrador, Entrenador")]
        public async Task<IActionResult> Entrenadores()
        {
            return View(await Task.Run(() => ListarEntrenadores()));
        }

        // Método para agregar un entrenador
        public void agregarEntrenador(Entrenador nuevoEntrenador)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spAgregarEntrenador", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", nuevoEntrenador.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", nuevoEntrenador.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", nuevoEntrenador.DNI);
                    cmd.Parameters.AddWithValue("@Telefono", nuevoEntrenador.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", nuevoEntrenador.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", nuevoEntrenador.Activo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al agregar el entrenador: " + ex.Message);
            }
        }

        public async Task<IActionResult> NuevoEntrenador()
        {
            // Retornar una vista vacía para agregar un nuevo entrenador
            return View(await Task.Run(() => new Entrenador()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoEntrenador(Entrenador entrenador)
        {
            // Asignar el valor de Activo a true directamente
            entrenador.Activo = true;

            // Verificar si el modelo es válido
            if (!ModelState.IsValid)
            {
                return View(await Task.Run(() => entrenador));
            }

            // Llama al método para agregar un nuevo entrenador a la base de datos
            agregarEntrenador(entrenador);

            // Redirige a la lista de entrenadores después de agregar
            return RedirectToAction(nameof(Entrenadores));
        }



        // Método para seleccionar un entrenador por ID
        public Entrenador seleccionarEntrenadorPorID(int idEntrenador)
        {
            Entrenador entrenador = new Entrenador();
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spSeleccionarEntrenador", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EntrenadorID", idEntrenador);
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        entrenador.EntrenadorID = dr.GetInt32(0);
                        entrenador.Nombre = dr.GetString(1);
                        entrenador.Apellido = dr.GetString(2);
                        entrenador.DNI = dr.GetString(3);
                        entrenador.Telefono = dr.IsDBNull(4) ? null : dr.GetString(4);
                        entrenador.Email = dr.IsDBNull(5) ? null : dr.GetString(5);
                        entrenador.Activo = dr.GetBoolean(6);
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al seleccionar el entrenador: " + ex.Message);
            }
            return entrenador;
        }

        public async Task<IActionResult> EditarEntrenador(int id)
        {
            // Obtiene los datos del entrenador por su ID
            Entrenador entrenador = seleccionarEntrenadorPorID(id);

            // Si no se encuentra el entrenador, retorna un 404
            if (entrenador == null)
            {
                return NotFound();
            }

            // Retorna la vista con los datos del entrenador
            return View(await Task.Run(() => entrenador));
        }


        // Método para editar un entrenador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarEntrenador(Entrenador entrenador)
        {
            // Verifica si el modelo es válido
            if (!ModelState.IsValid)
            {
                return View(await Task.Run(() => entrenador));
            }

            // Llama al método para actualizar los datos del entrenador en la base de datos
            actualizarEntrenador(entrenador);

            // Redirige a la lista de entrenadores después de actualizar
            return RedirectToAction(nameof(Entrenadores));
        }

        public void actualizarEntrenador(Entrenador entrenador)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(_config["ConnectionStrings:sql"]))
                {
                    cn.Open();
                    SqlCommand cmd = new SqlCommand("spActualizarEntrenador", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EntrenadorID", entrenador.EntrenadorID);
                    cmd.Parameters.AddWithValue("@Nombre", entrenador.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entrenador.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", entrenador.DNI);
                    cmd.Parameters.AddWithValue("@Telefono", entrenador.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", entrenador.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", entrenador.Activo);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al actualizar el entrenador: " + ex.Message);
            }
        }
    }
}
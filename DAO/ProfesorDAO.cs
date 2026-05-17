using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class ProfesorDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public ProfesorDAO()
            : this(new Conexion())
        {
        }

        public ProfesorDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<Profesor> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todos_profesores", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<Profesor>();
            while (reader.Read())
            {
                lista.Add(MapearProfesor(reader));
            }

            return lista;
        }

        public Profesor? ObtenerPorId(int id)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_profesor_por_id", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_profesor", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearProfesor(reader) : null;
        }

        public IEnumerable<Profesor> ObtenerPorEspecialidad(string especialidad)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_profesores_por_especialidad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_especialidad", especialidad);

            using var reader = command.ExecuteReader();

            var lista = new List<Profesor>();
            while (reader.Read())
            {
                lista.Add(MapearProfesor(reader));
            }

            return lista;
        }

        public IEnumerable<Profesor> Buscar(string busqueda)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_buscar_profesores", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_busqueda", busqueda);

            using var reader = command.ExecuteReader();

            var lista = new List<Profesor>();
            while (reader.Read())
            {
                lista.Add(MapearProfesor(reader));
            }

            return lista;
        }

        public bool Crear(Profesor profesor, out int profesorId)
        {
            profesorId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dni", profesor.DNI);
            command.Parameters.AddWithValue("@p_nombre", profesor.Nombre);
            command.Parameters.AddWithValue("@p_apellido", profesor.Apellido);
            command.Parameters.AddWithValue("@p_telefono", profesor.Telefono);
            command.Parameters.AddWithValue("@p_email", profesor.Email);
            command.Parameters.AddWithValue("@p_especialidad", profesor.Especialidad);
            command.Parameters.AddWithValue("@p_sueldo_base", profesor.SueldoMensual);

            var outputParam = new MySqlParameter("@p_profesor_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    profesorId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(Profesor profesor)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_profesor", profesor.IdPersona);
            command.Parameters.AddWithValue("@p_nombre", profesor.Nombre);
            command.Parameters.AddWithValue("@p_apellido", profesor.Apellido);
            command.Parameters.AddWithValue("@p_telefono", profesor.Telefono);
            command.Parameters.AddWithValue("@p_email", profesor.Email);
            command.Parameters.AddWithValue("@p_especialidad", profesor.Especialidad);
            command.Parameters.AddWithValue("@p_sueldo_base", profesor.SueldoMensual);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int profesorId, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_profesor", profesorId);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                mensaje = outputParam.Value?.ToString() ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Profesor MapearProfesor(MySqlDataReader reader)
        {
            return new Profesor
            {
                IdPersona = reader.GetInt32("id_profesor"),
                DNI = reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                Especialidad = reader.IsDBNull(reader.GetOrdinal("especialidad")) ? string.Empty : reader.GetString("especialidad"),
                SueldoMensual = reader.IsDBNull(reader.GetOrdinal("sueldo_base")) ? 0 : reader.GetDouble(reader.GetOrdinal("sueldo_base"))
            };
        }
    }
}

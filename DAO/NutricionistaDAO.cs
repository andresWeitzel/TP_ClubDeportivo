using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class NutricionistaDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public NutricionistaDAO()
            : this(new Conexion())
        {
        }

        public NutricionistaDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, string DNI, string Nombre, string Apellido, string Telefono, string Email, string Matricula)> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todos_nutricionistas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, string, string, string, string, string, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_nutricionista"),
                    reader.GetString("dni"),
                    reader.GetString("nombre"),
                    reader.GetString("apellido"),
                    reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                    reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                    reader.IsDBNull(reader.GetOrdinal("matricula")) ? string.Empty : reader.GetString("matricula")
                ));
            }

            return lista;
        }

        public (int Id, string DNI, string Nombre, string Apellido, string Telefono, string Email, string Matricula)? ObtenerPorId(int id)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_nutricionista_por_id", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_nutricionista", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return (
                    reader.GetInt32("id_nutricionista"),
                    reader.GetString("dni"),
                    reader.GetString("nombre"),
                    reader.GetString("apellido"),
                    reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                    reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                    reader.IsDBNull(reader.GetOrdinal("matricula")) ? string.Empty : reader.GetString("matricula")
                );
            }

            return null;
        }

        public IEnumerable<(int Id, string DNI, string Nombre, string Apellido, string Telefono, string Email, string Matricula)> Buscar(string busqueda)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_buscar_nutricionistas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_busqueda", busqueda);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, string, string, string, string, string, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_nutricionista"),
                    reader.GetString("dni"),
                    reader.GetString("nombre"),
                    reader.GetString("apellido"),
                    reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                    reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                    reader.IsDBNull(reader.GetOrdinal("matricula")) ? string.Empty : reader.GetString("matricula")
                ));
            }

            return lista;
        }

        public bool Crear(string dni, string nombre, string apellido, string telefono, string email, string matricula, out int nutricionistaId)
        {
            nutricionistaId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_nutricionista", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dni", dni);
            command.Parameters.AddWithValue("@p_nombre", nombre);
            command.Parameters.AddWithValue("@p_apellido", apellido);
            command.Parameters.AddWithValue("@p_telefono", telefono);
            command.Parameters.AddWithValue("@p_email", email);
            command.Parameters.AddWithValue("@p_matricula", matricula);

            var outputParam = new MySqlParameter("@p_nutricionista_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    nutricionistaId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int nutricionistaId, string nombre, string apellido, string telefono, string email, string matricula)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_nutricionista", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_nutricionista", nutricionistaId);
            command.Parameters.AddWithValue("@p_nombre", nombre);
            command.Parameters.AddWithValue("@p_apellido", apellido);
            command.Parameters.AddWithValue("@p_telefono", telefono);
            command.Parameters.AddWithValue("@p_email", email);
            command.Parameters.AddWithValue("@p_matricula", matricula);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int nutricionistaId, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_nutricionista", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_nutricionista", nutricionistaId);

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
    }
}

using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class SocioDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public SocioDAO()
            : this(new Conexion())
        {
        }

        public SocioDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<Socio> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_socios", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<Socio>();
            while (reader.Read())
            {
                lista.Add(MapearSocio(reader));
            }

            return lista;
        }

        public Socio? ObtenerPorId(int id)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_socio_por_id", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_socio", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearSocio(reader) : null;
        }

        public Socio? ObtenerPorDni(string dni)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_socio_por_dni", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dni", dni);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearSocio(reader) : null;
        }

        public bool Crear(Socio socio, out int socioId)
        {
            socioId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dni", socio.DNI);
            command.Parameters.AddWithValue("@p_nombre", socio.Nombre);
            command.Parameters.AddWithValue("@p_apellido", socio.Apellido);
            command.Parameters.AddWithValue("@p_telefono", socio.Telefono);
            command.Parameters.AddWithValue("@p_direccion", socio.Direccion);
            command.Parameters.AddWithValue("@p_email", socio.Email);

            var outputParam = new MySqlParameter("@p_socio_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    socioId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool ActualizarEstadoCuota(int socioId, string estado)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_estado_cuota", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);
            command.Parameters.AddWithValue("@p_estado", estado);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(Socio socio)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socio.IdSocio);
            command.Parameters.AddWithValue("@p_nombre", socio.Nombre);
            command.Parameters.AddWithValue("@p_apellido", socio.Apellido);
            command.Parameters.AddWithValue("@p_telefono", socio.Telefono);
            command.Parameters.AddWithValue("@p_direccion", socio.Direccion);
            command.Parameters.AddWithValue("@p_email", socio.Email);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int socioId, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                mensaje = outputParam.Value?.ToString() ?? string.Empty;
                return mensaje.Contains("eliminado", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static Socio MapearSocio(MySqlDataReader reader)
        {
            return new Socio
            {
                IdSocio = reader.GetInt32("id_socio"),
                DNI = reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                Direccion = reader.IsDBNull(reader.GetOrdinal("direccion")) ? string.Empty : reader.GetString("direccion"),
                Email = reader.IsDBNull(reader.GetOrdinal("email")) ? string.Empty : reader.GetString("email"),
                EstadoCuota = reader.IsDBNull(reader.GetOrdinal("estado_cuota")) ? string.Empty : reader.GetString("estado_cuota"),
                FechaAlta = reader.GetDateTime("fecha_alta")
            };
        }
    }
}

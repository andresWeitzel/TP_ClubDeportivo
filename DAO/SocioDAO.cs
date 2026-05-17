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

            const string sql = @"SELECT
                                    id_socio,
                                    dni,
                                    nombre,
                                    apellido,
                                    telefono,
                                    direccion,
                                    email,
                                    estado_cuota,
                                    fecha_alta
                                  FROM socios";

            using var command = new MySqlCommand(sql, connection);
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

            const string sql = @"SELECT
                                    id_socio,
                                    dni,
                                    nombre,
                                    apellido,
                                    telefono,
                                    direccion,
                                    email,
                                    estado_cuota,
                                    fecha_alta
                                  FROM socios
                                  WHERE id_socio = @id_socio
                                  LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id_socio", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearSocio(reader) : null;
        }

        public Socio? ObtenerPorDni(string dni)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                    id_socio,
                                    dni,
                                    nombre,
                                    apellido,
                                    telefono,
                                    direccion,
                                    email,
                                    estado_cuota,
                                    fecha_alta
                                  FROM socios
                                  WHERE dni = @dni
                                  LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dni", dni);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearSocio(reader) : null;
        }

        public bool Crear(Socio socio)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"INSERT INTO socios
                                  (dni, nombre, apellido, telefono, direccion, email, estado_cuota)
                                  VALUES
                                  (@dni, @nombre, @apellido, @telefono, @direccion, @email, @estado_cuota)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dni", socio.DNI);
            command.Parameters.AddWithValue("@nombre", socio.Nombre);
            command.Parameters.AddWithValue("@apellido", socio.Apellido);
            command.Parameters.AddWithValue("@telefono", socio.Telefono);
            command.Parameters.AddWithValue("@direccion", socio.Direccion);
            command.Parameters.AddWithValue("@email", socio.Email);
            command.Parameters.AddWithValue("@estado_cuota", string.IsNullOrWhiteSpace(socio.EstadoCuota) ? "AL_DIA" : socio.EstadoCuota);

            return command.ExecuteNonQuery() == 1;
        }

        public bool ActualizarEstadoCuota(int socioId, string estado)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"UPDATE socios
                                  SET estado_cuota = @estado_cuota
                                  WHERE id_socio = @id_socio";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@estado_cuota", estado);
            command.Parameters.AddWithValue("@id_socio", socioId);

            return command.ExecuteNonQuery() == 1;
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

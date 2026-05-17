using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class RutinaDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public RutinaDAO()
            : this(new Conexion())
        {
        }

        public RutinaDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int ProfesorId, string ProfesorNombre, string Descripcion, DateTime FechaCreacion, string Observaciones)> ObtenerTodas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todas_rutinas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, string, DateTime, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_rutina"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.IsDBNull(reader.GetOrdinal("descripcion")) ? string.Empty : reader.GetString("descripcion"),
                    reader.GetDateTime("fecha_creacion"),
                    reader.IsDBNull(reader.GetOrdinal("observaciones")) ? string.Empty : reader.GetString("observaciones")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int ProfesorId, string ProfesorNombre, string Descripcion, DateTime FechaCreacion, string Observaciones)> ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_rutinas_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, string, DateTime, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_rutina"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.IsDBNull(reader.GetOrdinal("descripcion")) ? string.Empty : reader.GetString("descripcion"),
                    reader.GetDateTime("fecha_creacion"),
                    reader.IsDBNull(reader.GetOrdinal("observaciones")) ? string.Empty : reader.GetString("observaciones")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int ProfesorId, string ProfesorNombre, string Descripcion, DateTime FechaCreacion, string Observaciones)> ObtenerPorProfesor(int profesorId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_rutinas_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, string, DateTime, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_rutina"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.IsDBNull(reader.GetOrdinal("descripcion")) ? string.Empty : reader.GetString("descripcion"),
                    reader.GetDateTime("fecha_creacion"),
                    reader.IsDBNull(reader.GetOrdinal("observaciones")) ? string.Empty : reader.GetString("observaciones")
                ));
            }

            return lista;
        }

        public bool Crear(int socioId, int profesorId, string descripcion, string observaciones, out int rutinaId)
        {
            rutinaId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_rutina", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_descripcion", string.IsNullOrEmpty(descripcion) ? DBNull.Value : descripcion);
            command.Parameters.AddWithValue("@p_observaciones", string.IsNullOrEmpty(observaciones) ? DBNull.Value : observaciones);

            var outputParam = new MySqlParameter("@p_rutina_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    rutinaId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int rutinaId, string descripcion, string observaciones)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_rutina", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_rutina", rutinaId);
            command.Parameters.AddWithValue("@p_descripcion", string.IsNullOrEmpty(descripcion) ? DBNull.Value : descripcion);
            command.Parameters.AddWithValue("@p_observaciones", string.IsNullOrEmpty(observaciones) ? DBNull.Value : observaciones);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int rutinaId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_rutina", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_rutina", rutinaId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }
    }
}

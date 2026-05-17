using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;

namespace TP_ClubDeportivo.DAO
{
    internal class AsistenciaDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public AsistenciaDAO()
            : this(new Conexion())
        {
        }

        public AsistenciaDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int ProfesorId, DateTime Fecha, bool Presente, string Firma)> ObtenerPorProfesor(int profesorId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_asistencias_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, DateTime, bool, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_asistencia"),
                    reader.GetInt32("profesor_id"),
                    reader.GetDateTime("fecha"),
                    reader.GetBoolean("presente"),
                    reader.IsDBNull(reader.GetOrdinal("firma")) ? string.Empty : reader.GetString("firma")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, DateTime Fecha, bool Presente)> ObtenerPorFecha(DateTime fecha)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_asistencias_por_fecha", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, DateTime, bool)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_asistencia"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetBoolean("presente")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, DateTime Fecha, bool Presente)> ObtenerRango(DateTime fechaInicio, DateTime fechaFin)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_asistencias_rango", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_fecha_inicio", fechaInicio.Date);
            command.Parameters.AddWithValue("@p_fecha_fin", fechaFin.Date);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, DateTime, bool)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_asistencia"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetBoolean("presente")
                ));
            }

            return lista;
        }

        public bool Registrar(int profesorId, DateTime fecha, bool presente, string firma, out int asistenciaId)
        {
            asistenciaId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_registrar_asistencia", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);
            command.Parameters.AddWithValue("@p_presente", presente);
            command.Parameters.AddWithValue("@p_firma", string.IsNullOrEmpty(firma) ? DBNull.Value : firma);

            var outputParam = new MySqlParameter("@p_asistencia_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    asistenciaId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int asistenciaId, bool presente, string firma)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_asistencia", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_asistencia", asistenciaId);
            command.Parameters.AddWithValue("@p_presente", presente);
            command.Parameters.AddWithValue("@p_firma", string.IsNullOrEmpty(firma) ? DBNull.Value : firma);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int asistenciaId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_asistencia", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_asistencia", asistenciaId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public (string ProfesorNombre, int Total, int Asistencias, int Inasistencias, double Porcentaje)? ObtenerReporte(int profesorId, DateTime fechaInicio, DateTime fechaFin)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_reporte_asistencias_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_fecha_inicio", fechaInicio.Date);
            command.Parameters.AddWithValue("@p_fecha_fin", fechaFin.Date);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return (
                    reader.GetString("profesor_nombre"),
                    reader.GetInt32("total_registros"),
                    reader.GetInt32("asistencias"),
                    reader.GetInt32("inasistencias"),
                    reader.GetDouble("porcentaje_asistencia")
                );
            }

            return null;
        }
    }
}

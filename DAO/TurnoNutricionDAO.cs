using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;

namespace TP_ClubDeportivo.DAO
{
    internal class TurnoNutricionDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public TurnoNutricionDAO()
            : this(new Conexion())
        {
        }

        public TurnoNutricionDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int NutricionistaId, string NutricionistaNombre, DateTime Fecha, TimeSpan Hora, string Estado)> ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_turnos_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, DateTime, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_turno"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("nutricionista_id"),
                    reader.GetString("nutricionista_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetTimeSpan("hora"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int NutricionistaId, string NutricionistaNombre, DateTime Fecha, TimeSpan Hora, string Estado)> ObtenerPorNutricionista(int nutricionistaId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_turnos_nutricionista", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_nutricionista_id", nutricionistaId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, DateTime, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_turno"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("nutricionista_id"),
                    reader.GetString("nutricionista_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetTimeSpan("hora"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, int NutricionistaId, string NutricionistaNombre, DateTime Fecha, TimeSpan Hora, string Estado)> ObtenerPorFecha(DateTime fecha)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_turnos_por_fecha", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, string, DateTime, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_turno"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetInt32("nutricionista_id"),
                    reader.GetString("nutricionista_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetTimeSpan("hora"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, DateTime Fecha, TimeSpan Hora)> ObtenerDisponibles(DateTime fecha, int nutricionistaId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_turnos_disponibles", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);
            command.Parameters.AddWithValue("@p_nutricionista_id", nutricionistaId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, DateTime, TimeSpan)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_turno"),
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetDateTime("fecha"),
                    reader.GetTimeSpan("hora")
                ));
            }

            return lista;
        }

        public bool Crear(int socioId, int nutricionistaId, DateTime fecha, TimeSpan hora, string estado, out int turnoId)
        {
            turnoId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_turno_nutricion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);
            command.Parameters.AddWithValue("@p_nutricionista_id", nutricionistaId);
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);
            command.Parameters.AddWithValue("@p_hora", hora);
            command.Parameters.AddWithValue("@p_estado", estado);

            var outputParam = new MySqlParameter("@p_turno_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    turnoId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int turnoId, DateTime fecha, TimeSpan hora, string estado)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_turno_nutricion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_turno", turnoId);
            command.Parameters.AddWithValue("@p_fecha", fecha.Date);
            command.Parameters.AddWithValue("@p_hora", hora);
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

        public bool Confirmar(int turnoId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_confirmar_turno_nutricion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_turno", turnoId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Cancelar(int turnoId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_cancelar_turno_nutricion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_turno", turnoId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int turnoId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_turno_nutricion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_turno", turnoId);

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

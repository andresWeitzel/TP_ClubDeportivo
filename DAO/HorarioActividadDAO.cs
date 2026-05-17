using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class HorarioActividadDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public HorarioActividadDAO()
            : this(new Conexion())
        {
        }

        public HorarioActividadDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<HorarioActividad> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todos_horarios", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<HorarioActividad>();
            while (reader.Read())
            {
                lista.Add(MapearHorarioActividad(reader));
            }

            return lista;
        }

        public IEnumerable<HorarioActividad> ObtenerPorProfesor(int profesorId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_horarios_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);

            using var reader = command.ExecuteReader();

            var lista = new List<HorarioActividad>();
            while (reader.Read())
            {
                lista.Add(new HorarioActividad
                {
                    IdHorario = reader.GetInt32("id_horario"),
                    ProfesorId = reader.GetInt32("profesor_id"),
                    ProfesorNombre = string.Empty,
                    DiaSemana = reader.GetString("dia_semana"),
                    HoraInicio = reader.GetTimeSpan("hora_inicio"),
                    HoraFin = reader.GetTimeSpan("hora_fin"),
                    Actividad = reader.GetString("actividad")
                });
            }

            return lista;
        }

        public IEnumerable<HorarioActividad> ObtenerPorDia(string dia)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_horarios_por_dia", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dia_semana", dia);

            using var reader = command.ExecuteReader();

            var lista = new List<HorarioActividad>();
            while (reader.Read())
            {
                lista.Add(MapearHorarioActividad(reader));
            }

            return lista;
        }

        public bool Crear(int profesorId, string dia, TimeSpan horaInicio, TimeSpan horaFin, string actividad, out int horarioId)
        {
            horarioId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_horario_clase", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_dia_semana", dia);
            command.Parameters.AddWithValue("@p_hora_inicio", horaInicio);
            command.Parameters.AddWithValue("@p_hora_fin", horaFin);
            command.Parameters.AddWithValue("@p_actividad", actividad);

            var outputParam = new MySqlParameter("@p_horario_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    horarioId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int horarioId, string dia, TimeSpan horaInicio, TimeSpan horaFin, string actividad)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_horario_clase", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_horario", horarioId);
            command.Parameters.AddWithValue("@p_dia_semana", dia);
            command.Parameters.AddWithValue("@p_hora_inicio", horaInicio);
            command.Parameters.AddWithValue("@p_hora_fin", horaFin);
            command.Parameters.AddWithValue("@p_actividad", actividad);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int horarioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_horario_clase", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_horario", horarioId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        private static HorarioActividad MapearHorarioActividad(MySqlDataReader reader)
        {
            return new HorarioActividad
            {
                IdHorario = reader.GetInt32("id_horario"),
                ProfesorId = reader.GetInt32("profesor_id"),
                ProfesorNombre = reader.IsDBNull(reader.GetOrdinal("profesor_nombre")) ? string.Empty : reader.GetString("profesor_nombre"),
                DiaSemana = reader.GetString("dia_semana"),
                HoraInicio = reader.GetTimeSpan("hora_inicio"),
                HoraFin = reader.GetTimeSpan("hora_fin"),
                Actividad = reader.GetString("actividad")
            };
        }
    }
}

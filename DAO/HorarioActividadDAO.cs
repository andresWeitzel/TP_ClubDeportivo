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
                lista.Add(MapearHorarioActividad(reader));
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

        public bool Crear(int profesorId, int actividadId, string dia, TimeSpan horaInicio, TimeSpan horaFin, out int horarioId)
        {
            horarioId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_horario_actividad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_actividad_id", actividadId);
            command.Parameters.AddWithValue("@p_dia_semana", dia);
            command.Parameters.AddWithValue("@p_hora_inicio", horaInicio);
            command.Parameters.AddWithValue("@p_hora_fin", horaFin);

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

        public bool Actualizar(int horarioId, int actividadId, string dia, TimeSpan horaInicio, TimeSpan horaFin)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_horario_actividad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_horario", horarioId);
            command.Parameters.AddWithValue("@p_actividad_id", actividadId);
            command.Parameters.AddWithValue("@p_dia_semana", dia);
            command.Parameters.AddWithValue("@p_hora_inicio", horaInicio);
            command.Parameters.AddWithValue("@p_hora_fin", horaFin);

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

            using var command = new MySqlCommand("sp_eliminar_horario_actividad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_horario", horarioId);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string LeerColumnaOpcional(MySqlDataReader reader, string columna)
        {
            try
            {
                var ord = reader.GetOrdinal(columna);
                return reader.IsDBNull(ord) ? string.Empty : reader.GetString(ord);
            }
            catch (IndexOutOfRangeException)
            {
                return string.Empty;
            }
        }

        private static HorarioActividad MapearHorarioActividad(MySqlDataReader reader)
        {
            return new HorarioActividad
            {
                IdHorario = reader.GetInt32("id_horario"),
                ProfesorId = reader.GetInt32("profesor_id"),
                ProfesorNombre = LeerColumnaOpcional(reader, "profesor_nombre"),
                ActividadId = reader.GetInt32("actividad_id"),
                Actividad = reader.GetString("actividad"),
                DiaSemana = reader.GetString("dia_semana"),
                HoraInicio = reader.GetTimeSpan("hora_inicio"),
                HoraFin = reader.GetTimeSpan("hora_fin")
            };
        }
    }
}

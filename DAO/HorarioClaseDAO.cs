using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;

namespace TP_ClubDeportivo.DAO
{
    internal class HorarioClaseDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public HorarioClaseDAO()
            : this(new Conexion())
        {
        }

        public HorarioClaseDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int ProfesorId, string Dia, TimeSpan HoraInicio, TimeSpan HoraFin, string Actividad)> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todos_horarios", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, TimeSpan, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_horario"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("dia_semana"),
                    reader.GetTimeSpan("hora_inicio"),
                    reader.GetTimeSpan("hora_fin"),
                    reader.GetString("actividad")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string Dia, TimeSpan HoraInicio, TimeSpan HoraFin, string Actividad)> ObtenerPorProfesor(int profesorId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_horarios_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, TimeSpan, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_horario"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("dia_semana"),
                    reader.GetTimeSpan("hora_inicio"),
                    reader.GetTimeSpan("hora_fin"),
                    reader.GetString("actividad")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, string Dia, TimeSpan HoraInicio, TimeSpan HoraFin, string Actividad)> ObtenerPorDia(string dia)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_horarios_por_dia", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dia_semana", dia);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, string, TimeSpan, TimeSpan, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_horario"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.GetString("dia_semana"),
                    reader.GetTimeSpan("hora_inicio"),
                    reader.GetTimeSpan("hora_fin"),
                    reader.GetString("actividad")
                ));
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
    }
}

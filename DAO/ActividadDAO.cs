using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class ActividadDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public ActividadDAO()
            : this(new Conexion())
        {
        }

        public ActividadDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<Actividad> ObtenerActivas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_actividades", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<Actividad>();
            while (reader.Read())
            {
                lista.Add(MapearActividad(reader));
            }

            return lista;
        }

        public bool VerificarCupo(int actividadId, int? excluirVisitanteId, out bool hayCupo, out int ocupados, out int cupoMaximo, out string nombreActividad)
        {
            hayCupo = false;
            ocupados = 0;
            cupoMaximo = 0;
            nombreActividad = string.Empty;

            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_verificar_cupo_actividad", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_actividad_id", actividadId);
            command.Parameters.AddWithValue("@p_excluir_visitante_id", excluirVisitanteId.HasValue ? excluirVisitanteId.Value : DBNull.Value);

            command.Parameters.Add(new MySqlParameter("@p_hay_cupo", MySqlDbType.Bit) { Direction = ParameterDirection.Output });
            command.Parameters.Add(new MySqlParameter("@p_ocupados", MySqlDbType.Int32) { Direction = ParameterDirection.Output });
            command.Parameters.Add(new MySqlParameter("@p_cupo_maximo", MySqlDbType.Int32) { Direction = ParameterDirection.Output });
            command.Parameters.Add(new MySqlParameter("@p_nombre_actividad", MySqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
            var hayCupoParam = command.Parameters["@p_hay_cupo"];
            var ocupadosParam = command.Parameters["@p_ocupados"];
            var cupoParam = command.Parameters["@p_cupo_maximo"];
            var nombreParam = command.Parameters["@p_nombre_actividad"];

            try
            {
                command.ExecuteNonQuery();
                hayCupo = LeerEntero(hayCupoParam.Value) != 0;
                ocupados = LeerEntero(ocupadosParam.Value);
                cupoMaximo = LeerEntero(cupoParam.Value);
                nombreActividad = nombreParam.Value?.ToString() ?? string.Empty;
                return cupoMaximo > 0;
            }
            catch
            {
                return false;
            }
        }

        private static int LeerEntero(object? valor) =>
            valor switch
            {
                null or DBNull => 0,
                int i => i,
                long l => (int)l,
                bool b => b ? 1 : 0,
                _ => int.TryParse(valor.ToString(), out var n) ? n : 0
            };

        private static Actividad MapearActividad(MySqlDataReader reader)
        {
            return new Actividad
            {
                IdActividad = reader.GetInt32("id_actividad"),
                Nombre = reader.GetString("nombre"),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? string.Empty : reader.GetString("descripcion"),
                CupoMaximo = reader.GetInt32("cupo_maximo"),
                PrecioVisitante = reader.GetDecimal("precio_visitante"),
                Activa = reader.GetBoolean("activa"),
                OcupadosHoy = reader.GetInt32("ocupados_hoy")
            };
        }
    }
}

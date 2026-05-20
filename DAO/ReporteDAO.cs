using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class ReporteDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public ReporteDAO()
            : this(new Conexion())
        {
        }

        public ReporteDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<CuotaReporte> ObtenerCuotasPorVencer(int dias)
        {
            if (dias < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(dias), "Los días deben ser al menos 1.");
            }

            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_cuotas_por_vencer", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dias", dias);

            using var reader = command.ExecuteReader();
            var lista = new List<CuotaReporte>();
            while (reader.Read())
            {
                lista.Add(MapearCuotaReporte(reader, "dias_para_vencer"));
            }

            return lista;
        }

        public IEnumerable<CuotaReporte> ObtenerCuotasVencidas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_cuotas_vencidas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();
            var lista = new List<CuotaReporte>();
            while (reader.Read())
            {
                lista.Add(MapearCuotaReporte(reader, "dias_vencidos"));
            }

            return lista;
        }

        public ResultadoControlVencimiento? EjecutarControlVencimiento()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_controlar_vencimiento_cuotas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var cuotasParam = new MySqlParameter("@p_cuotas_en_mora", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            var sociosParam = new MySqlParameter("@p_socios_suspendidos", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(cuotasParam);
            command.Parameters.Add(sociosParam);

            try
            {
                command.ExecuteNonQuery();
                return new ResultadoControlVencimiento
                {
                    CuotasEnMora = LeerEntero(cuotasParam.Value),
                    SociosSuspendidos = LeerEntero(sociosParam.Value)
                };
            }
            catch
            {
                return null;
            }
        }

        private static int LeerEntero(object? valor) =>
            valor switch
            {
                null or DBNull => 0,
                int i => i,
                long l => (int)l,
                _ => int.TryParse(valor.ToString(), out var n) ? n : 0
            };

        private static CuotaReporte MapearCuotaReporte(MySqlDataReader reader, string columnaDias)
        {
            return new CuotaReporte
            {
                IdSocio = reader.GetInt32("id_socio"),
                Dni = reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.GetString("apellido"),
                EstadoCuotaSocio = reader.GetString("estado_cuota"),
                IdCuota = reader.GetInt32("id_cuota"),
                Monto = reader.GetDecimal("monto"),
                FechaVencimiento = reader.GetDateTime("fecha_vencimiento"),
                Dias = reader.GetInt32(columnaDias)
            };
        }
    }
}

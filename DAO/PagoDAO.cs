using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class PagoDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public PagoDAO()
            : this(new Conexion())
        {
        }

        public PagoDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public bool RegistrarPagoSocio(int socioId, int cuotaId, decimal monto, string medioPago, string concepto, out int pagoId)
        {
            pagoId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_registrar_pago_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);
            command.Parameters.AddWithValue("@p_cuota_id", cuotaId);
            command.Parameters.AddWithValue("@p_monto", monto);
            command.Parameters.AddWithValue("@p_medio_pago", medioPago);
            command.Parameters.AddWithValue("@p_concepto", concepto);

            var outputParam = new MySqlParameter("@p_pago_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    pagoId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Pago> ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_pagos_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);

            using var reader = command.ExecuteReader();
            var pagos = new List<Pago>();
            while (reader.Read())
            {
                pagos.Add(MapearPago(reader));
            }

            return pagos;
        }

        public bool RegistrarPagoVisitante(int visitanteId, decimal monto, string medioPago, string concepto, out int pagoId)
        {
            pagoId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_registrar_pago_visitante", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_visitante_id", visitanteId);
            command.Parameters.AddWithValue("@p_monto", monto);
            command.Parameters.AddWithValue("@p_medio_pago", medioPago);
            command.Parameters.AddWithValue("@p_concepto", concepto);

            var outputParam = new MySqlParameter("@p_pago_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    pagoId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Pago> ObtenerPorVisitante(int visitanteId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_pagos_visitante", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_visitante_id", visitanteId);

            using var reader = command.ExecuteReader();
            var pagos = new List<Pago>();
            while (reader.Read())
            {
                pagos.Add(MapearPago(reader));
            }

            return pagos;
        }

        private static Pago MapearPago(MySqlDataReader reader)
        {
            return new Pago
            {
                IdPago = reader.GetInt32("id_pago"),
                Tipo = reader.GetString("tipo"),
                SocioId = reader.IsDBNull(reader.GetOrdinal("socio_id")) ? null : reader.GetInt32("socio_id"),
                CuotaId = reader.IsDBNull(reader.GetOrdinal("cuota_id")) ? null : reader.GetInt32("cuota_id"),
                VisitanteId = reader.IsDBNull(reader.GetOrdinal("visitante_id")) ? null : reader.GetInt32("visitante_id"),
                Monto = reader.GetDecimal("monto"),
                FechaPago = reader.GetDateTime("fecha_pago"),
                MedioPago = reader.IsDBNull(reader.GetOrdinal("medio_pago")) ? string.Empty : reader.GetString("medio_pago"),
                Concepto = reader.IsDBNull(reader.GetOrdinal("concepto")) ? string.Empty : reader.GetString("concepto")
            };
        }
    }
}

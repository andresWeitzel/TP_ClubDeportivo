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

        public bool Crear(Pago pago)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"INSERT INTO pagos
                                  (tipo, socio_id, cuota_id, visitante_id, monto, fecha_pago, medio_pago, concepto)
                                  VALUES
                                  (@tipo, @socio_id, @cuota_id, @visitante_id, @monto, @fecha_pago, @medio_pago, @concepto)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@tipo", pago.Tipo);
            command.Parameters.AddWithValue("@socio_id", pago.SocioId.HasValue ? pago.SocioId.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@cuota_id", pago.CuotaId.HasValue ? pago.CuotaId.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@visitante_id", pago.VisitanteId.HasValue ? pago.VisitanteId.Value : (object)DBNull.Value);
            command.Parameters.AddWithValue("@monto", pago.Monto);
            command.Parameters.AddWithValue("@fecha_pago", pago.FechaPago == default ? DateTime.Now : pago.FechaPago);
            command.Parameters.AddWithValue("@medio_pago", pago.MedioPago);
            command.Parameters.AddWithValue("@concepto", pago.Concepto);

            return command.ExecuteNonQuery() == 1;
        }

        public IEnumerable<Pago> ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id_pago, tipo, socio_id, cuota_id, visitante_id, monto, fecha_pago, medio_pago, concepto
                                  FROM pagos
                                  WHERE socio_id = @socio_id
                                  ORDER BY fecha_pago DESC";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", socioId);

            using var reader = command.ExecuteReader();
            var pagos = new List<Pago>();
            while (reader.Read())
            {
                pagos.Add(MapearPago(reader));
            }

            return pagos;
        }

        public IEnumerable<Pago> ObtenerPorVisitante(int visitanteId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id_pago, tipo, socio_id, cuota_id, visitante_id, monto, fecha_pago, medio_pago, concepto
                                  FROM pagos
                                  WHERE visitante_id = @visitante_id
                                  ORDER BY fecha_pago DESC";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@visitante_id", visitanteId);

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

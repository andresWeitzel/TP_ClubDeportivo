using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class CuotaDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public CuotaDAO()
            : this(new Conexion())
        {
        }

        public CuotaDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<Cuota> ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                    id_cuota,
                                    socio_id,
                                    monto,
                                    fecha_emision,
                                    fecha_vencimiento,
                                    estado,
                                    en_mora
                                  FROM cuotas
                                  WHERE socio_id = @socio_id
                                  ORDER BY fecha_vencimiento DESC";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", socioId);

            using var reader = command.ExecuteReader();
            var lista = new List<Cuota>();
            while (reader.Read())
            {
                lista.Add(MapearCuota(reader));
            }

            return lista;
        }

        public Cuota? ObtenerUltimaPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                    id_cuota,
                                    socio_id,
                                    monto,
                                    fecha_emision,
                                    fecha_vencimiento,
                                    estado,
                                    en_mora
                                  FROM cuotas
                                  WHERE socio_id = @socio_id
                                  ORDER BY fecha_vencimiento DESC
                                  LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", socioId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearCuota(reader) : null;
        }

        public bool Crear(Cuota cuota)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"INSERT INTO cuotas
                                  (socio_id, monto, fecha_emision, fecha_vencimiento, estado, en_mora)
                                  VALUES
                                  (@socio_id, @monto, @fecha_emision, @fecha_vencimiento, @estado, @en_mora)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", cuota.SocioId);
            command.Parameters.AddWithValue("@monto", cuota.Monto);
            command.Parameters.AddWithValue("@fecha_emision", cuota.FechaEmision);
            command.Parameters.AddWithValue("@fecha_vencimiento", cuota.FechaVencimiento);
            command.Parameters.AddWithValue("@estado", string.IsNullOrWhiteSpace(cuota.Estado) ? "AL_DIA" : cuota.Estado);
            command.Parameters.AddWithValue("@en_mora", cuota.EnMora);

            return command.ExecuteNonQuery() == 1;
        }

        public bool ActualizarEstado(int cuotaId, string estado, bool enMora)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"UPDATE cuotas
                                  SET estado = @estado,
                                      en_mora = @en_mora
                                  WHERE id_cuota = @id_cuota";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@estado", estado);
            command.Parameters.AddWithValue("@en_mora", enMora);
            command.Parameters.AddWithValue("@id_cuota", cuotaId);

            return command.ExecuteNonQuery() == 1;
        }

        public IEnumerable<Cuota> ObtenerPorVencerEnDias(int dias)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                    id_cuota,
                                    socio_id,
                                    monto,
                                    fecha_emision,
                                    fecha_vencimiento,
                                    estado,
                                    en_mora
                                  FROM cuotas
                                  WHERE fecha_vencimiento BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL @dias DAY)
                                  ORDER BY fecha_vencimiento";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dias", dias);

            using var reader = command.ExecuteReader();
            var lista = new List<Cuota>();
            while (reader.Read())
            {
                lista.Add(MapearCuota(reader));
            }

            return lista;
        }

        public IEnumerable<Cuota> ObtenerVencidas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                    id_cuota,
                                    socio_id,
                                    monto,
                                    fecha_emision,
                                    fecha_vencimiento,
                                    estado,
                                    en_mora
                                  FROM cuotas
                                  WHERE fecha_vencimiento < CURDATE()
                                  ORDER BY fecha_vencimiento";

            using var command = new MySqlCommand(sql, connection);
            using var reader = command.ExecuteReader();
            var lista = new List<Cuota>();
            while (reader.Read())
            {
                lista.Add(MapearCuota(reader));
            }

            return lista;
        }

        private static Cuota MapearCuota(MySqlDataReader reader)
        {
            return new Cuota
            {
                IdCuota = reader.GetInt32("id_cuota"),
                SocioId = reader.GetInt32("socio_id"),
                Monto = reader.GetDecimal("monto"),
                FechaEmision = reader.GetDateTime("fecha_emision"),
                FechaVencimiento = reader.GetDateTime("fecha_vencimiento"),
                Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? string.Empty : reader.GetString("estado"),
                EnMora = reader.GetBoolean("en_mora")
            };
        }
    }
}

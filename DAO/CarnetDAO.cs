using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class CarnetDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public CarnetDAO()
            : this(new Conexion())
        {
        }

        public CarnetDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public bool Crear(Carnet carnet)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"INSERT INTO carnets
                                  (socio_id, numero, fecha_emision, fecha_vencimiento, foto)
                                  VALUES
                                  (@socio_id, @numero, @fecha_emision, @fecha_vencimiento, @foto)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", carnet.SocioId);
            command.Parameters.AddWithValue("@numero", carnet.Numero);
            command.Parameters.AddWithValue("@fecha_emision", carnet.FechaEmision);
            command.Parameters.AddWithValue("@fecha_vencimiento", carnet.FechaVencimiento);
            command.Parameters.AddWithValue("@foto", carnet.Foto);

            return command.ExecuteNonQuery() == 1;
        }

        public Carnet? ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id_carnet, socio_id, numero, fecha_emision, fecha_vencimiento, foto
                                  FROM carnets
                                  WHERE socio_id = @socio_id
                                  LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@socio_id", socioId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearCarnet(reader) : null;
        }

        private static Carnet MapearCarnet(MySqlDataReader reader)
        {
            return new Carnet
            {
                IdCarnet = reader.GetInt32("id_carnet"),
                SocioId = reader.GetInt32("socio_id"),
                Numero = reader.GetString("numero"),
                FechaEmision = reader.GetDateTime("fecha_emision"),
                FechaVencimiento = reader.GetDateTime("fecha_vencimiento"),
                Foto = reader.IsDBNull(reader.GetOrdinal("foto")) ? string.Empty : reader.GetString("foto")
            };
        }
    }
}

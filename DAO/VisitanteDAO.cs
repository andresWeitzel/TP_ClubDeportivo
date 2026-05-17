using System.Collections.Generic;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class VisitanteDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public VisitanteDAO()
            : this(new Conexion())
        {
        }

        public VisitanteDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public bool Crear(Visitante visitante)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"INSERT INTO visitantes
                                  (dni, nombre, apellido, telefono, actividad, fecha_ingreso, pago_diario_monto)
                                  VALUES
                                  (@dni, @nombre, @apellido, @telefono, @actividad, @fecha_ingreso, @pago_diario_monto)";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dni", visitante.DNI);
            command.Parameters.AddWithValue("@nombre", visitante.Nombre);
            command.Parameters.AddWithValue("@apellido", visitante.Apellido);
            command.Parameters.AddWithValue("@telefono", visitante.Telefono);
            command.Parameters.AddWithValue("@actividad", visitante.Actividad);
            command.Parameters.AddWithValue("@fecha_ingreso", visitante.FechaIngreso == default ? DateTime.Now : visitante.FechaIngreso);
            command.Parameters.AddWithValue("@pago_diario_monto", visitante.PagoDiarioMonto);

            return command.ExecuteNonQuery() == 1;
        }

        public Visitante? ObtenerPorId(int id)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id_visitante, dni, nombre, apellido, telefono, actividad, fecha_ingreso, pago_diario_monto
                                  FROM visitantes
                                  WHERE id_visitante = @id_visitante
                                  LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id_visitante", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearVisitante(reader) : null;
        }

        public IEnumerable<Visitante> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT id_visitante, dni, nombre, apellido, telefono, actividad, fecha_ingreso, pago_diario_monto
                                  FROM visitantes
                                  ORDER BY fecha_ingreso DESC";

            using var command = new MySqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            var lista = new List<Visitante>();
            while (reader.Read())
            {
                lista.Add(MapearVisitante(reader));
            }

            return lista;
        }

        private static Visitante MapearVisitante(MySqlDataReader reader)
        {
            return new Visitante
            {
                IdVisitante = reader.GetInt32("id_visitante"),
                DNI = reader.IsDBNull(reader.GetOrdinal("dni")) ? string.Empty : reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido")) ? string.Empty : reader.GetString("apellido"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                Actividad = reader.IsDBNull(reader.GetOrdinal("actividad")) ? string.Empty : reader.GetString("actividad"),
                FechaIngreso = reader.GetDateTime("fecha_ingreso"),
                PagoDiarioMonto = reader.GetDecimal("pago_diario_monto")
            };
        }
    }
}

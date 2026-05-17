using System.Collections.Generic;
using System.Data;
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

        public bool Crear(Visitante visitante, out int visitanteId)
        {
            visitanteId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_visitante", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_dni", visitante.DNI);
            command.Parameters.AddWithValue("@p_nombre", visitante.Nombre);
            command.Parameters.AddWithValue("@p_apellido", visitante.Apellido);
            command.Parameters.AddWithValue("@p_telefono", visitante.Telefono);
            command.Parameters.AddWithValue("@p_actividad", visitante.Actividad);
            command.Parameters.AddWithValue("@p_pago_diario_monto", visitante.PagoDiarioMonto);

            var outputParam = new MySqlParameter("@p_visitante_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    visitanteId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public Visitante? ObtenerPorId(int id)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_visitante_por_id", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_visitante", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapearVisitante(reader) : null;
        }

        public IEnumerable<Visitante> ObtenerTodos()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_visitantes", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<Visitante>();
            while (reader.Read())
            {
                lista.Add(MapearVisitante(reader));
            }

            return lista;
        }

        public bool Actualizar(Visitante visitante)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_visitante", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_visitante", visitante.IdVisitante);
            command.Parameters.AddWithValue("@p_dni", visitante.DNI);
            command.Parameters.AddWithValue("@p_nombre", visitante.Nombre);
            command.Parameters.AddWithValue("@p_apellido", visitante.Apellido);
            command.Parameters.AddWithValue("@p_telefono", visitante.Telefono);
            command.Parameters.AddWithValue("@p_actividad", visitante.Actividad);
            command.Parameters.AddWithValue("@p_pago_diario_monto", visitante.PagoDiarioMonto);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int visitanteId, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_visitante", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_visitante", visitanteId);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                mensaje = outputParam.Value?.ToString() ?? string.Empty;
                return mensaje.Contains("eliminado", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<VisitanteListado> ObtenerListado()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_visitantes_listado", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<VisitanteListado>();
            while (reader.Read())
            {
                lista.Add(MapearVisitanteListado(reader));
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

        private static VisitanteListado MapearVisitanteListado(MySqlDataReader reader)
        {
            var tienePago = reader.GetInt32(reader.GetOrdinal("tiene_pago")) == 1;
            return new VisitanteListado
            {
                IdVisitante = reader.GetInt32("id_visitante"),
                Dni = reader.IsDBNull(reader.GetOrdinal("dni")) ? string.Empty : reader.GetString("dni"),
                Nombre = reader.GetString("nombre"),
                Apellido = reader.IsDBNull(reader.GetOrdinal("apellido")) ? string.Empty : reader.GetString("apellido"),
                Telefono = reader.IsDBNull(reader.GetOrdinal("telefono")) ? string.Empty : reader.GetString("telefono"),
                Actividad = reader.IsDBNull(reader.GetOrdinal("actividad")) ? string.Empty : reader.GetString("actividad"),
                FechaIngreso = reader.GetDateTime("fecha_ingreso"),
                Monto = reader.GetDecimal("monto"),
                MedioPago = reader.IsDBNull(reader.GetOrdinal("medio_pago")) ? string.Empty : reader.GetString("medio_pago"),
                PagoRegistrado = tienePago ? "Sí" : "No"
            };
        }
    }
}

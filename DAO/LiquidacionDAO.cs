using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;

namespace TP_ClubDeportivo.DAO
{
    internal class LiquidacionDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public LiquidacionDAO()
            : this(new Conexion())
        {
        }

        public LiquidacionDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, int Mes, int Anio, decimal MontoBruto, decimal Descuentos, decimal MontoNeto, DateTime? FechaPago, string Estado)> ObtenerTodas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todas_liquidaciones", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, int, decimal, decimal, decimal, DateTime?, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_liquidacion"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.GetInt32("mes"),
                    reader.GetInt32("anio"),
                    reader.GetDecimal("monto_bruto"),
                    reader.GetDecimal("descuentos"),
                    reader.GetDecimal("monto_neto"),
                    reader.IsDBNull(reader.GetOrdinal("fecha_pago")) ? null : (DateTime?)reader.GetDateTime("fecha_pago"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, int Mes, int Anio, decimal MontoBruto, decimal Descuentos, decimal MontoNeto, DateTime? FechaPago, string Estado)> ObtenerPorProfesor(int profesorId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_liquidaciones_profesor", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, int, int, decimal, decimal, decimal, DateTime?, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_liquidacion"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.GetInt32("mes"),
                    reader.GetInt32("anio"),
                    reader.GetDecimal("monto_bruto"),
                    reader.GetDecimal("descuentos"),
                    reader.GetDecimal("monto_neto"),
                    reader.IsDBNull(reader.GetOrdinal("fecha_pago")) ? null : (DateTime?)reader.GetDateTime("fecha_pago"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public IEnumerable<(int Id, int ProfesorId, string ProfesorNombre, string Especialidad, int Mes, int Anio, decimal MontoBruto, decimal Descuentos, decimal MontoNeto, DateTime? FechaPago, string Estado)> ObtenerPorPeriodo(int mes, int anio)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_liquidaciones_periodo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_mes", mes);
            command.Parameters.AddWithValue("@p_anio", anio);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, string, int, int, decimal, decimal, decimal, DateTime?, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("id_liquidacion"),
                    reader.GetInt32("profesor_id"),
                    reader.GetString("profesor_nombre"),
                    reader.IsDBNull(reader.GetOrdinal("especialidad")) ? string.Empty : reader.GetString("especialidad"),
                    reader.GetInt32("mes"),
                    reader.GetInt32("anio"),
                    reader.GetDecimal("monto_bruto"),
                    reader.GetDecimal("descuentos"),
                    reader.GetDecimal("monto_neto"),
                    reader.IsDBNull(reader.GetOrdinal("fecha_pago")) ? null : reader.GetDateTime("fecha_pago"),
                    reader.GetString("estado")
                ));
            }

            return lista;
        }

        public bool Crear(int profesorId, int mes, int anio, decimal montoBruto, decimal descuentos, decimal montoNeto, string estado, out int liquidacionId)
        {
            liquidacionId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_liquidacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_profesor_id", profesorId);
            command.Parameters.AddWithValue("@p_mes", mes);
            command.Parameters.AddWithValue("@p_anio", anio);
            command.Parameters.AddWithValue("@p_monto_bruto", montoBruto);
            command.Parameters.AddWithValue("@p_descuentos", descuentos);
            command.Parameters.AddWithValue("@p_monto_neto", montoNeto);
            command.Parameters.AddWithValue("@p_estado", estado);

            var outputParam = new MySqlParameter("@p_liquidacion_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    liquidacionId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int liquidacionId, decimal montoBruto, decimal descuentos, decimal montoNeto)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_liquidacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_liquidacion", liquidacionId);
            command.Parameters.AddWithValue("@p_monto_bruto", montoBruto);
            command.Parameters.AddWithValue("@p_descuentos", descuentos);
            command.Parameters.AddWithValue("@p_monto_neto", montoNeto);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Pagar(int liquidacionId, DateTime fechaPago, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_pagar_liquidacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_liquidacion", liquidacionId);
            command.Parameters.AddWithValue("@p_fecha_pago", fechaPago.Date);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                mensaje = outputParam.Value?.ToString() ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int liquidacionId, out string mensaje)
        {
            mensaje = string.Empty;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_liquidacion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_liquidacion", liquidacionId);

            var outputParam = new MySqlParameter("@p_mensaje", MySqlDbType.String, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                mensaje = outputParam.Value?.ToString() ?? string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public (int CantidadProfesores, decimal TotalBruto, decimal TotalDescuentos, decimal TotalNeto, int PagosRealizados, int PagosPendientes)? ObtenerReportePeriodo(int mes, int anio)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_reporte_liquidaciones_periodo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_mes", mes);
            command.Parameters.AddWithValue("@p_anio", anio);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return (
                    reader.GetInt32("cantidad_profesores"),
                    reader.GetDecimal("total_bruto"),
                    reader.GetDecimal("total_descuentos"),
                    reader.GetDecimal("total_neto"),
                    reader.GetInt32("pagos_realizados"),
                    reader.GetInt32("pagos_pendientes")
                );
            }

            return null;
        }
    }
}

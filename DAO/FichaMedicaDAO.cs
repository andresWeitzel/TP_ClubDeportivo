using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class FichaMedicaDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public FichaMedicaDAO()
            : this(new Conexion())
        {
        }

        public FichaMedicaDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, decimal Peso, decimal Altura, string Alergias, string Medicacion, string Observaciones, string CargaPermitida)> ObtenerTodas()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_todas_fichas_medicas", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, decimal, decimal, string, string, string, string)>();
            while (reader.Read())
            {
                lista.Add(MapearFichaMedica(reader));
            }

            return lista;
        }

        public (int Id, int SocioId, string SocioNombre, decimal Peso, decimal Altura, string Alergias, string Medicacion, string Observaciones, string CargaPermitida)? ObtenerPorSocio(int socioId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_ficha_medica_socio", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapearFichaMedica(reader);
            }

            return null;
        }

        public IEnumerable<(int Id, int SocioId, string SocioNombre, decimal Peso, decimal Altura, string Alergias, string Medicacion, string Observaciones, string CargaPermitida)> ObtenerPorAlergia(string alergia)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_obtener_fichas_por_alergias", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_alergia", alergia);

            using var reader = command.ExecuteReader();

            var lista = new List<(int, int, string, decimal, decimal, string, string, string, string)>();
            while (reader.Read())
            {
                lista.Add(MapearFichaMedica(reader));
            }

            return lista;
        }

        public bool Crear(int socioId, decimal peso, decimal altura, string alergias, string medicacion, string observaciones, string cargaPermitida, out int fichaId)
        {
            fichaId = 0;
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_crear_ficha_medica", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_socio_id", socioId);
            command.Parameters.AddWithValue("@p_peso", peso);
            command.Parameters.AddWithValue("@p_altura", altura);
            command.Parameters.AddWithValue("@p_alergias", string.IsNullOrEmpty(alergias) ? DBNull.Value : alergias);
            command.Parameters.AddWithValue("@p_medicacion", string.IsNullOrEmpty(medicacion) ? DBNull.Value : medicacion);
            command.Parameters.AddWithValue("@p_observaciones", string.IsNullOrEmpty(observaciones) ? DBNull.Value : observaciones);
            command.Parameters.AddWithValue("@p_carga_permitida", string.IsNullOrEmpty(cargaPermitida) ? DBNull.Value : cargaPermitida);

            var outputParam = new MySqlParameter("@p_ficha_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputParam);

            try
            {
                command.ExecuteNonQuery();
                if (int.TryParse(outputParam.Value?.ToString(), out var id))
                {
                    fichaId = id;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Actualizar(int fichaId, decimal peso, decimal altura, string alergias, string medicacion, string observaciones, string cargaPermitida)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_actualizar_ficha_medica", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_ficha", fichaId);
            command.Parameters.AddWithValue("@p_peso", peso);
            command.Parameters.AddWithValue("@p_altura", altura);
            command.Parameters.AddWithValue("@p_alergias", string.IsNullOrEmpty(alergias) ? DBNull.Value : alergias);
            command.Parameters.AddWithValue("@p_medicacion", string.IsNullOrEmpty(medicacion) ? DBNull.Value : medicacion);
            command.Parameters.AddWithValue("@p_observaciones", string.IsNullOrEmpty(observaciones) ? DBNull.Value : observaciones);
            command.Parameters.AddWithValue("@p_carga_permitida", string.IsNullOrEmpty(cargaPermitida) ? DBNull.Value : cargaPermitida);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public bool Eliminar(int fichaId)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_eliminar_ficha_medica", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@p_id_ficha", fichaId);

            try
            {
                return command.ExecuteNonQuery() >= 1;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<(int SocioId, string SocioNombre, decimal Peso, decimal Altura, decimal IMC, string CategoriaIMC, string CargaPermitida)> ObtenerReporteIMC()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("sp_reporte_imc_socios", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = command.ExecuteReader();

            var lista = new List<(int, string, decimal, decimal, decimal, string, string)>();
            while (reader.Read())
            {
                lista.Add((
                    reader.GetInt32("socio_id"),
                    reader.GetString("socio_nombre"),
                    reader.GetDecimal("peso"),
                    reader.GetDecimal("altura"),
                    reader.GetDecimal("imc"),
                    reader.GetString("categoria_imc"),
                    reader.IsDBNull(reader.GetOrdinal("carga_permitida")) ? string.Empty : reader.GetString("carga_permitida")
                ));
            }

            return lista;
        }

        private static (int, int, string, decimal, decimal, string, string, string, string) MapearFichaMedica(MySqlDataReader reader)
        {
            return (
                reader.GetInt32("id_ficha"),
                reader.GetInt32("socio_id"),
                reader.GetString("socio_nombre"),
                reader.IsDBNull(reader.GetOrdinal("peso")) ? 0 : reader.GetDecimal("peso"),
                reader.IsDBNull(reader.GetOrdinal("altura")) ? 0 : reader.GetDecimal("altura"),
                reader.IsDBNull(reader.GetOrdinal("alergias")) ? string.Empty : reader.GetString("alergias"),
                reader.IsDBNull(reader.GetOrdinal("medicacion")) ? string.Empty : reader.GetString("medicacion"),
                reader.IsDBNull(reader.GetOrdinal("observaciones")) ? string.Empty : reader.GetString("observaciones"),
                reader.IsDBNull(reader.GetOrdinal("carga_permitida")) ? string.Empty : reader.GetString("carga_permitida")
            );
        }
    }
}

using System.Data;
using MySqlConnector;
using TP_ClubDeportivo.Data;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.DAO
{
    internal class UsuarioDAO
    {
        private readonly IConexionFactory _conexionFactory;

        public UsuarioDAO()
            : this(new Conexion())
        {
        }

        public UsuarioDAO(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory;
        }

        public bool TestConexion()
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();
            return connection.State == ConnectionState.Open;
        }

        public Usuario? ObtenerPorUsername(string username)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            const string sql = @"SELECT
                                   u.CodUsu AS id_usuario,
                                   u.NombreUsu AS username,
                                   u.PassUsu AS password,
                                   r.NomRol AS rol,
                                   u.FechaRegistro AS fecha_registro
                                 FROM usuario u
                                 INNER JOIN roles r ON u.RolUsu = r.RolUsu
                                 WHERE u.NombreUsu = @username
                                 LIMIT 1";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Usuario
            {
                IdUsuario = reader.GetInt32("id_usuario"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                Rol = reader.GetString("rol"),
                FechaRegistro = reader.GetDateTime("fecha_registro")
            };
        }

        public Usuario? Login(string username, string password)
        {
            using var connection = _conexionFactory.ObtenerConexion();
            connection.Open();

            using var command = new MySqlCommand("IngresoLogin", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Usu", username);
            command.Parameters.AddWithValue("@Pass", password);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Usuario
            {
                IdUsuario = reader.GetInt32("id_usuario"),
                Username = reader.GetString("username"),
                Password = reader.GetString("password"),
                Rol = reader.GetString("rol"),
                FechaRegistro = reader.GetDateTime("fecha_registro")
            };
        }
    }
}

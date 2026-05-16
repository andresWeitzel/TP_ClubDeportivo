using MySqlConnector;

namespace TP_ClubDeportivo.Data
{
    public class Conexion : IConexionFactory
    {
        private readonly string cadenaConexion =
            "Server=localhost;" +
            "Database=db_club_deportivo;" +
            "User=root;" +
            "Password=;" +
            "Port=3306;";

        public MySqlConnection ObtenerConexion()
        {
            return new MySqlConnection(cadenaConexion);
        }
    }
}
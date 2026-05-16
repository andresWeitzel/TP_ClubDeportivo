using MySqlConnector;

namespace TP_ClubDeportivo.Data
{
    public interface IConexionFactory
    {
        MySqlConnection ObtenerConexion();
    }
}

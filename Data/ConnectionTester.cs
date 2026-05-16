using System;

namespace TP_ClubDeportivo.Data
{
    public class ConnectionTester
    {
        private readonly IConexionFactory _factory;

        public ConnectionTester(IConexionFactory factory)
        {
            _factory = factory;
        }

        public bool Test(out string message)
        {
            try
            {
                using var conn = _factory.ObtenerConexion();
                conn.Open();
                message = "Conexión exitosa.";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}

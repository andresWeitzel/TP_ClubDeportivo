namespace TP_ClubDeportivo.Models
{
    internal class Usuario
    {
        public int IdUsuario { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public void Registrar()
        {
            Console.WriteLine("Usuario registrado.");
        }
    }
}
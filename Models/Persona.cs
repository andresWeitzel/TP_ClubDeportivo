namespace TP_ClubDeportivo.Models
{
    internal class Persona
    {
        public string DNI { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public string Telefono { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public Persona()
        {
        }

        public Persona(
            string dni,
            string nombre,
            string apellido,
            DateTime fechaNacimiento,
            string telefono,
            string direccion)
        {
            DNI = dni;
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            Telefono = telefono;
            Direccion = direccion;
        }

        public virtual void ObtenerDatos()
        {
            Console.WriteLine($"Persona: {Nombre} {Apellido}");
        }
    }
}
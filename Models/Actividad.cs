namespace TP_ClubDeportivo.Models
{
    internal class Actividad
    {
        public string IdActividad { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CupoMaximo { get; set; }

        public HorarioActividad Horario { get; set; } = new HorarioActividad();

        public void InscribirSocio()
        {
            Console.WriteLine("Socio inscripto en la actividad.");
        }
    }
}
namespace TP_ClubDeportivo.Models
{
    internal class AsistenciaProfesor
    {
        public DateTime Fecha { get; set; }

        public TimeSpan HoraEntrada { get; set; }

        public TimeSpan HoraSalida { get; set; }

        public string Firma { get; set; } = string.Empty;

        public void Registrar()
        {
            Console.WriteLine("Asistencia registrada.");
        }
    }
}
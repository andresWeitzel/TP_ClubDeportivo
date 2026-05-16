namespace TP_ClubDeportivo.Models
{
    internal class HorarioActividad
    {
        public string DiaSemana { get; set; } = string.Empty;

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }
    }
}
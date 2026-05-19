namespace TP_ClubDeportivo.Models
{
    internal class HorarioActividad
    {
        public int IdHorario { get; set; }

        public int ProfesorId { get; set; }


        public string DiaSemana { get; set; } = string.Empty;

        public TimeSpan HoraInicio { get; set; }

        public TimeSpan HoraFin { get; set; }

    }
}
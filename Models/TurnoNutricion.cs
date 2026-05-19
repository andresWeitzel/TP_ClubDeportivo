namespace TP_ClubDeportivo.Models
{
    internal class TurnoNutricion
    {
        public int IdTurno { get; set; } 

        public DateTime Fecha { get; set; }

        public TimeSpan Hora { get; set; }

        public string Estado { get; set; } = string.Empty;

        public void Reservar()
        {
            Console.WriteLine("Turno reservado.");
        }

        public void Cancelar()
        {
            Console.WriteLine("Turno cancelado.");
        }
    }
}
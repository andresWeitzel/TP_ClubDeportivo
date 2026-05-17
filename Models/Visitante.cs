namespace TP_ClubDeportivo.Models
{
    internal class Visitante : Persona
    {
        public int IdVisitante { get; set; }

        public DateTime FechaIngreso { get; set; }

        public string Actividad { get; set; } = string.Empty;

        public decimal PagoDiarioMonto { get; set; }

        public void RegistrarIngreso()
        {
            Console.WriteLine("Ingreso registrado.");
        }
    }
}
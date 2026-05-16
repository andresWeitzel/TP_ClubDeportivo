namespace TP_ClubDeportivo.Models
{
    internal class Rutina
    {

        public string IdRutina { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public List<string> Ejercicios { get; set; } = new List<string>();

        public string Observaciones { get; set; } = string.Empty;

        public void Modificar()
        {
            Console.WriteLine("Rutina modificada.");
        }
    }
}
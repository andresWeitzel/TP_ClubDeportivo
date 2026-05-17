namespace TP_ClubDeportivo.Models
{
    internal class Carnet
    {
        public int IdCarnet { get; set; }

        public int SocioId { get; set; }

        public string Numero { get; set; } = string.Empty;

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public string Foto { get; set; } = string.Empty;

        public void Emitir()
        {
            Console.WriteLine("Carnet emitido.");
        }

        public void Renovar()
        {
            Console.WriteLine("Carnet renovado.");
        }
    }
}
namespace TP_ClubDeportivo.Models
{
    internal class CuotaMensual
    {
        public string Periodo { get; set; } = string.Empty;

        public DateTime FechaVencimiento { get; set; }

        public bool EnMora { get; set; }

        public double CalcularMora()
        {
            return 0;
        }

        public bool EstaVencida()
        {
            return FechaVencimiento < DateTime.Now;
        }
    }
}
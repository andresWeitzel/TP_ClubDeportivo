namespace TP_ClubDeportivo.Models
{
    internal class Cuota
    {
        public int IdCuota { get; set; }

        public int SocioId { get; set; }

        public int VisitanteId { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public bool EnMora { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Periodo { get; set; } = string.Empty;

        public string TipoCuota { get; set; } = string.Empty;

        public decimal CalcularMora(decimal tasaDiaria = 0.005m)
        {
            if (!EstaVencida())
            {
                return 0m;
            }

            var diasVencidos = (DateTime.Now.Date - FechaVencimiento.Date).Days;
            return Math.Round(Monto * tasaDiaria * diasVencidos, 2);
        }

        public bool EstaVencida()
        {
            return FechaVencimiento.Date < DateTime.Now.Date;
        }
    }
}
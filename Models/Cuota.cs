namespace TP_ClubDeportivo.Models
{
    internal class Cuota
    {
        public int IdCuota { get; set; }

        public int SocioId { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaEmision { get; set; }

        public DateTime FechaVencimiento { get; set; }

        public bool EnMora { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string Periodo { get; set; } = string.Empty;

        public string TipoCuota { get; set; } = string.Empty;

        public bool EstaVencida()
        {
            return FechaVencimiento.Date < DateTime.Now.Date;
        }

        /// <summary>
        /// CU-03 E1: cuota vencida, marcada en mora o con estado VENCIDA.
        /// </summary>
        public bool RequiereRecargo()
        {
            if (Estado == "PAGADA")
            {
                return false;
            }

            return EnMora
                || Estado.Equals("VENCIDA", StringComparison.OrdinalIgnoreCase)
                || EstaVencida();
        }

        public decimal CalcularMora(decimal tasaDiaria = 0.005m)
        {
            if (!EstaVencida())
            {
                return 0m;
            }

            var diasVencidos = (DateTime.Now.Date - FechaVencimiento.Date).Days;
            return Math.Round(Monto * tasaDiaria * diasVencidos, 2);
        }

        /// <summary>
        /// Recargo por mora: interés por días vencidos o, si aplica, mínimo del 10% del monto base.
        /// </summary>
        public decimal ObtenerRecargo(decimal tasaDiaria = 0.005m, decimal porcentajeMinimoMora = 0.10m)
        {
            if (!RequiereRecargo())
            {
                return 0m;
            }

            var porDiasVencidos = CalcularMora(tasaDiaria);
            if (EnMora || Estado.Equals("VENCIDA", StringComparison.OrdinalIgnoreCase))
            {
                var minimo = Math.Round(Monto * porcentajeMinimoMora, 2);
                return Math.Max(porDiasVencidos, minimo);
            }

            return porDiasVencidos;
        }

        public decimal MontoTotalConRecargo(decimal tasaDiaria = 0.005m, decimal porcentajeMinimoMora = 0.10m) =>
            Monto + ObtenerRecargo(tasaDiaria, porcentajeMinimoMora);
    }
}
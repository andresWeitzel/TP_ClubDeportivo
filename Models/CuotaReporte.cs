namespace TP_ClubDeportivo.Models
{
    /// <summary>
    /// Proyección para reportes RF-15 (por vencer) y RF-16 (vencidas).
    /// </summary>
    internal class CuotaReporte
    {
        public int IdSocio { get; set; }

        public string Dni { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string EstadoCuotaSocio { get; set; } = string.Empty;

        public int IdCuota { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaVencimiento { get; set; }

        /// <summary>Días hasta vencer (reporte por vencer) o días de atraso (reporte vencidas).</summary>
        public int Dias { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }
}

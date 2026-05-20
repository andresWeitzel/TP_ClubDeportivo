namespace TP_ClubDeportivo.Models
{
    /// <summary>
    /// Resultado de CU-04 — control diario de vencimiento de cuotas.
    /// </summary>
    internal class ResultadoControlVencimiento
    {
        public int CuotasEnMora { get; set; }

        public int SociosSuspendidos { get; set; }
    }
}

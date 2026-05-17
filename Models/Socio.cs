namespace TP_ClubDeportivo.Models
{
    internal class Socio : Persona
    {
        public int IdSocio { get; set; }

        public string NumeroSocio { get; set; } = string.Empty;

        public string EstadoCuota { get; set; } = string.Empty;

        public DateTime FechaAlta { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool VerificarEstadoCuota()
        {
            return EstadoCuota.Equals("AL_DIA", StringComparison.OrdinalIgnoreCase) || EstadoCuota.Equals("Al día", StringComparison.OrdinalIgnoreCase);
        }

        public void PagarCuota()
        {
            Console.WriteLine("Cuota pagada.");
        }

        public void SolicitarCarnet()
        {
            Console.WriteLine("Carnet solicitado.");
        }
    }
}
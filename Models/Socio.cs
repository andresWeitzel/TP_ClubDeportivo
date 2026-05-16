namespace TP_ClubDeportivo.Models
{
    internal class Socio : Persona
    {
        public string NumeroSocio { get; set; } = string.Empty;

        public string EstadoCuota { get; set; } = string.Empty;

        public DateTime FechaAlta { get; set; }

        public bool VerificarEstadoCuota()
        {
            return EstadoCuota == "Al día";
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
namespace TP_ClubDeportivo.Models
{
    internal class Pago
    {
        public int IdPago { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public int? SocioId { get; set; }

        public int? CuotaId { get; set; }

        public int? VisitanteId { get; set; }

        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public string MedioPago { get; set; } = string.Empty;

        public string Concepto { get; set; } = string.Empty;

        public void Registrar()
        {
            Console.WriteLine("Pago registrado.");
        }

        public void EmitirComprobante()
        {
            Console.WriteLine("Comprobante emitido.");
        }
    }
}
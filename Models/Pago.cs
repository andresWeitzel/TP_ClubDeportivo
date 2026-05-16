namespace TP_ClubDeportivo.Models
{
    internal class Pago
    {
        public string IdPago { get; set; } = string.Empty;

        public double Monto { get; set; }

        public DateTime FechaPago { get; set; }

        public string MedioPago { get; set; } = string.Empty;

        public void Registrar()
        {
            Console.WriteLine("Pago registrado.");
        }
    }
}
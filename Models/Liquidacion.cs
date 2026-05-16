namespace TP_ClubDeportivo.Models
{
    internal class Liquidacion
    {
        public string IdLiquidacion { get; set; } = string.Empty;

        public string Periodo { get; set; } = string.Empty;

        public double MontoBruto { get; set; }

        public double Descuentos { get; set; }

        public double MontoNeto { get; set; }

        public DateTime FechaPago { get; set; }

        public void Calcular()
        {
            Console.WriteLine("Liquidación calculada.");
        }

        public void EmitirRecibo()
        {
            Console.WriteLine("Recibo emitido.");
        }
    }
}
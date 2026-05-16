namespace TP_ClubDeportivo.Models
{
    internal class Empleado : Persona
    {
        public string IdEmpleado { get; set; } = string.Empty;

        public double SalarioBase { get; set; }

        public DateTime FechaIngreso { get; set; }

        public void CobrarLiquidacion()
        {
            Console.WriteLine("Liquidación cobrada.");
        }
    }
}
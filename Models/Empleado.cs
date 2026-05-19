namespace TP_ClubDeportivo.Models
{
    internal class Empleado : Persona
    {
        public int IdEmpleado { get; set; } 

        public double SalarioBase { get; set; }

        public DateTime FechaIngreso { get; set; }

        public void CobrarLiquidacion()
        {
            Console.WriteLine("Liquidación cobrada.");
        }
    }
}
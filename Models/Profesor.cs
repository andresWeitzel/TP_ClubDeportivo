namespace TP_ClubDeportivo.Models
{
    internal class Profesor : Persona
    {
        public string Especialidad { get; set; } = string.Empty;

        public int Legajo { get; set; }

        public double SueldoMensual { get; set; }

        public void DictarClase()
        {
            Console.WriteLine("Clase dictada.");
        }

        public void SupervisarSalonGeneral()
        {
            Console.WriteLine("Supervisión realizada.");
        }

        public void CrearRutina()
        {
            Console.WriteLine("Rutina creada.");
        }

        public void FirmarPlanillaAsistencia()
        {
            Console.WriteLine("Asistencia registrada.");
        }
    }
}
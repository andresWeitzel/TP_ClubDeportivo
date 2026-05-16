namespace TP_ClubDeportivo.Models
{
    internal class Nutricionista : Persona
    {
        public string Matricula { get; set; } = string.Empty;

        public void AtenderTurno(TurnoNutricion turno)
        {
            Console.WriteLine("Turno atendido.");
        }

        public void DefinirCargaActividad(FichaMedica ficha)
        {
            Console.WriteLine("Carga de actividad definida.");
        }
    }
}
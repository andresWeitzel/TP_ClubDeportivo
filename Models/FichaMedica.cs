namespace TP_ClubDeportivo.Models
{
    internal class FichaMedica
    {
        public int IdFicha { get; set; } 

        public string Antecedentes { get; set; } = string.Empty;

        public string Alergias { get; set; } = string.Empty;

        public string Medicacion { get; set; } = string.Empty;

        public string CargaActividadPermitida { get; set; } = string.Empty;

        public void Actualizar()
        {
            Console.WriteLine("Ficha médica actualizada.");
        }
    }
}
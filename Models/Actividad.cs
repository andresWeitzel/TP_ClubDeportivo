namespace TP_ClubDeportivo.Models
{
    internal class Actividad
    {
        public int IdActividad { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public int CupoMaximo { get; set; }

        public decimal PrecioVisitante { get; set; }

        public bool Activa { get; set; } = true;

        public int OcupadosHoy { get; set; }

        public bool TieneCupo => OcupadosHoy < CupoMaximo;

        public string TextoLista => $"{Nombre} ({OcupadosHoy}/{CupoMaximo} hoy)";

        public override string ToString() => TextoLista;
    }
}

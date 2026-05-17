namespace TP_ClubDeportivo.Models
{
    /// <summary>
    /// Proyección para grilla: solo campos de visitantes + último pago (sin herencia Persona).
    /// </summary>
    internal class VisitanteListado
    {
        public int IdVisitante { get; set; }

        public string Dni { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Actividad { get; set; } = string.Empty;

        public DateTime FechaIngreso { get; set; }

        public decimal Monto { get; set; }

        public string MedioPago { get; set; } = string.Empty;

        public string PagoRegistrado { get; set; } = string.Empty;
    }
}

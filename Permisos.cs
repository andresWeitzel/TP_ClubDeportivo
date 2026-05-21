using TP_ClubDeportivo.Forms;

namespace TP_ClubDeportivo
{
    /// <summary>
    /// Matriz de acceso por rol según actores de los casos de uso del análisis.
    /// </summary>
    internal static class Permisos
    {
        public const string Administrador = "Administrador";
        public const string Empleado = "Empleado";
        public const string Profesor = "Profesor";
        public const string Nutricionista = "Nutricionista";
        public const string Socio = "Socio";
        public const string Visitante = "Visitante";

        public enum Modulo
        {
            Socios,
            Visitantes,
            CobroCuota,
            Carnets,
            FirmarAsistencia,
            Rutinas,
            TurnosNutricion,
            Reportes,
            ReporteAsistenciaProfesores,
            ControlVencimientoCuotas,
            LiquidarHaberes
        }

        public static bool PuedeIngresarAlSistema()
        {
            return Sesion.TieneRol(Administrador, Empleado, Profesor, Nutricionista);
        }

        public static bool PuedeAcceder(Modulo modulo)
        {
            return modulo switch
            {
                Modulo.Socios => Sesion.TieneRol(Administrador, Empleado),
                Modulo.Visitantes => Sesion.TieneRol(Administrador, Empleado),
                Modulo.CobroCuota => Sesion.TieneRol(Administrador, Empleado),
                Modulo.Carnets => Sesion.TieneRol(Administrador, Empleado),
                Modulo.FirmarAsistencia => Sesion.TieneRol(Administrador, Profesor),
                Modulo.Rutinas => Sesion.TieneRol(Administrador, Profesor),
                Modulo.TurnosNutricion => Sesion.TieneRol(Administrador, Nutricionista),
                Modulo.Reportes => Sesion.TieneRol(Administrador, Empleado),
                Modulo.ReporteAsistenciaProfesores => Sesion.TieneRol(Administrador),
                Modulo.ControlVencimientoCuotas => Sesion.TieneRol(Administrador, Empleado),
                Modulo.LiquidarHaberes => Sesion.TieneRol(Administrador),
                _ => false
            };
        }

        public static string DescripcionModulo(Modulo modulo) => modulo switch
        {
            Modulo.Socios => "Gestión de socios (CU-01)",
            Modulo.Visitantes => "Registro de visitantes (CU-02)",
            Modulo.CobroCuota => "Cobro de cuota mensual (CU-03)",
            Modulo.Carnets => "Emisión y renovación de carnets (RF-03)",
            Modulo.FirmarAsistencia => "Firmar asistencia (CU-05)",
            Modulo.Rutinas => "Confeccionar rutina (CU-06)",
            Modulo.TurnosNutricion => "Turnos de nutrición (CU-07)",
            Modulo.Reportes => "Reportes de cuotas (CU-09)",
            Modulo.ReporteAsistenciaProfesores => "Reporte de asistencia de profesores (RF-17)",
            Modulo.ControlVencimientoCuotas => "Control de vencimiento de cuotas (CU-04)",
            Modulo.LiquidarHaberes => "Liquidar haberes (CU-08)",
            _ => "este módulo"
        };

        public static string MensajeAccesoDenegado(Modulo modulo)
        {
            var rol = Sesion.UsuarioActual?.Rol ?? "sin rol";
            return $"Su rol ({rol}) no tiene permiso para acceder a {DescripcionModulo(modulo)}.\n\n" +
                   "Si necesita esta función, contacte a un administrador.";
        }

        public static bool IntentarAcceder(Modulo modulo, out string mensaje)
        {
            if (PuedeAcceder(modulo))
            {
                mensaje = string.Empty;
                return true;
            }

            mensaje = MensajeAccesoDenegado(modulo);
            return false;
        }

        public static Modulo? ObtenerModuloFormulario(Type tipoFormulario)
        {
            if (tipoFormulario == typeof(FormSocios))
            {
                return Modulo.Socios;
            }

            if (tipoFormulario == typeof(FormVisitantes))
            {
                return Modulo.Visitantes;
            }

            if (tipoFormulario == typeof(FormCobroCuota))
            {
                return Modulo.CobroCuota;
            }

            if (tipoFormulario == typeof(FormCarnets))
            {
                return Modulo.Carnets;
            }

            if (tipoFormulario == typeof(FormAsistencias))
            {
                return Modulo.FirmarAsistencia;
            }

            if (tipoFormulario == typeof(FormRutinas))
            {
                return Modulo.Rutinas;
            }

            if (tipoFormulario == typeof(FormTurnosNutricion))
            {
                return Modulo.TurnosNutricion;
            }

            if (tipoFormulario == typeof(FormReportes))
            {
                return Modulo.Reportes;
            }

            if (tipoFormulario == typeof(FormLiquidarHaberes))
            {
                return Modulo.LiquidarHaberes;
            }

            return null;
        }

        public static bool IntentarAbrirFormulario<T>(out string mensaje) where T : Form, new()
        {
            var modulo = ObtenerModuloFormulario(typeof(T));
            if (modulo is null)
            {
                mensaje = string.Empty;
                return true;
            }

            return IntentarAcceder(modulo.Value, out mensaje);
        }

        /// <summary>Segunda validación al abrir el formulario (por si se invoca fuera del menú principal).</summary>
        public static bool ValidarAccesoAlAbrir(Form form, Modulo modulo)
        {
            if (IntentarAcceder(modulo, out var mensaje))
            {
                return true;
            }

            MessageBox.Show(mensaje, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            form.Close();
            return false;
        }
    }
}

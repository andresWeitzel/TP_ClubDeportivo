using System.Drawing;
using System.Windows.Forms;

namespace TP_ClubDeportivo.Forms
{
    /// <summary>
    /// Panel principal reducido cuando el usuario no tiene módulos asignados (ej. Socio, Visitante).
    /// </summary>
    internal class FormAccesoDenegado : Form
    {
        public FormAccesoDenegado(string mensaje)
        {
            Text = "Club Deportivo — Acceso";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(520, 320);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32),
                BackColor = UiTheme.Tarjeta
            };

            panel.Controls.Add(new Label
            {
                Text = "Acceso no disponible",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = UiTheme.Texto
            });

            panel.Controls.Add(new Label
            {
                Text = mensaje,
                Dock = DockStyle.Fill,
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.TextoSecundario
            });

            var btnCerrar = new Button
            {
                Text = "Cerrar sesión",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            UiTheme.AplicarBotonPrimario(btnCerrar);
            btnCerrar.Click += (_, _) => Close();

            panel.Controls.Add(btnCerrar);
            Controls.Add(panel);
        }
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace TP_ClubDeportivo.Forms
{
    public class FormPrincipal : Form
    {
        private readonly Panel panelContenido;
        private readonly Panel panelDashboard;
        private readonly ToolStripStatusLabel lblStatus;

        public FormPrincipal()
        {
            Text = "Club Deportivo — Sistema de Gestión";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(960, 600);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelSidebar = CrearSidebar();
            var panelDerecho = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Fondo };

            var panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 84,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(28, 16, 28, 12)
            };
            panelTop.Paint += (_, e) =>
            {
                using var pen = new Pen(UiTheme.Borde);
                e.Graphics.DrawLine(pen, 0, panelTop.Height - 1, panelTop.Width, panelTop.Height - 1);
            };

            var lblTituloPagina = new Label
            {
                Text = "Panel principal",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = UiTheme.Texto,
                AutoSize = true,
                Location = new Point(28, 14)
            };

            var lblSubtituloPagina = new Label
            {
                Text = $"Sesión iniciada como {Sesion.UsuarioActual?.Username}",
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario,
                AutoSize = true,
                Location = new Point(30, 50)
            };

            var lblRol = new Label
            {
                Text = Sesion.UsuarioActual?.Rol ?? "",
                AutoSize = false,
                Size = new Size(140, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = UiTheme.PrimarioClaro,
                ForeColor = UiTheme.Primario,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            panelTop.Resize += (_, _) => lblRol.Location = new Point(panelTop.Width - lblRol.Width - 28, 28);
            lblRol.Location = new Point(800, 28);

            panelTop.Controls.AddRange([lblTituloPagina, lblSubtituloPagina, lblRol]);

            panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(28, 20, 28, 16),
                BackColor = UiTheme.Fondo,
                AutoScroll = false
            };

            panelDashboard = CrearDashboard();
            panelContenido.Controls.Add(panelDashboard);

            var statusStrip = new StatusStrip
            {
                BackColor = UiTheme.Tarjeta,
                SizingGrip = false
            };
            lblStatus = new ToolStripStatusLabel($"Conectado · {Sesion.UsuarioActual?.Rol}")
            {
                ForeColor = UiTheme.TextoSecundario
            };
            statusStrip.Items.Add(lblStatus);

            panelDerecho.Controls.Add(panelContenido);
            panelDerecho.Controls.Add(panelTop);
            panelDerecho.Controls.Add(statusStrip);

            Controls.Add(panelDerecho);
            Controls.Add(panelSidebar);
        }

        private Panel CrearSidebar()
        {
            var sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = UiTheme.Primario,
                Padding = new Padding(0, 8, 0, 12)
            };
            sidebar.Paint += (s, e) => UiTheme.PintarFondoGradiente((Panel)s!, e);

            var lblMarca = new Label
            {
                Text = "Club Deportivo",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 56,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0)
            };

            var lblNav = new Label
            {
                Text = "MENÚ",
                ForeColor = Color.FromArgb(180, 210, 245),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(20, 0, 0, 4)
            };

            var panelNav = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 4, 8, 0)
            };

            var btnSocios = UiTheme.CrearBotonSidebar("Socios", (_, _) => AbrirFormulario<FormSocios>());
            var btnVisitantes = UiTheme.CrearBotonSidebar("Visitantes", (_, _) => AbrirFormulario<FormVisitantes>());
            var btnCuotas = UiTheme.CrearBotonSidebar("Cobrar cuota", (_, _) => AbrirFormulario<FormCobroCuota>());
            var btnFirmarAsistencia = UiTheme.CrearBotonSidebar("Firmar asistencia", (_, _) => AbrirFormulario<FormAsistencias>());
            var btnRutinas = UiTheme.CrearBotonSidebar("Confeccionar rutina", (_, _) => AbrirFormulario<FormRutinas>());
            var btnTurnosNutricion = UiTheme.CrearBotonSidebar("Turnos nutrición", (_, _) => AbrirFormulario<FormTurnosNutricion>());
            var btnReportes = UiTheme.CrearBotonSidebar("Reportes", (_, _) => AbrirFormulario<FormReportes>());
            var btnCarnets = UiTheme.CrearBotonSidebar("Carnets", (_, _) => AbrirFormulario<FormCarnets>());

            panelNav.Controls.Add(btnReportes);
            panelNav.Controls.Add(btnTurnosNutricion);
            panelNav.Controls.Add(btnRutinas);
            panelNav.Controls.Add(btnFirmarAsistencia);
            panelNav.Controls.Add(btnCarnets);
            panelNav.Controls.Add(btnCuotas);
            panelNav.Controls.Add(btnVisitantes);
            panelNav.Controls.Add(btnSocios);

            var btnSalir = new Button
            {
                Text = "  Cerrar sesión",
                Dock = DockStyle.Bottom,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(220, 235, 255),
                BackColor = UiTheme.PrimarioOscuro,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 0, 0)
            };
            btnSalir.FlatAppearance.BorderSize = 0;
            btnSalir.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 40, 40);
            btnSalir.Click += (_, _) => Close();

            sidebar.Controls.Add(panelNav);
            sidebar.Controls.Add(btnSalir);
            sidebar.Controls.Add(lblNav);
            sidebar.Controls.Add(lblMarca);

            return sidebar;
        }

        private Panel CrearDashboard()
        {
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.Fondo,
                Padding = new Padding(0, 0, 8, 0)
            };

            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = UiTheme.Fondo,
                Padding = new Padding(0, 0, 0, 8)
            };

            panelHeader.Controls.Add(new Label
            {
                Text = $"Hola, {Sesion.UsuarioActual?.Username}",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = UiTheme.Texto,
                AutoSize = true,
                Location = new Point(0, 0)
            });

            panelHeader.Controls.Add(new Label
            {
                Text = "Elegí un módulo para comenzar o usá el menú lateral.",
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario,
                AutoSize = true,
                Location = new Point(2, 48)
            });

            var panelInfo = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = UiTheme.PrimarioClaro,
                Padding = new Padding(20, 12, 20, 12),
                Margin = new Padding(0, 16, 0, 0)
            };

            panelInfo.Controls.Add(new Label
            {
                Text = "Tip: desde el menú lateral podés acceder a cualquier módulo en cualquier momento.",
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.PrimarioOscuro,
                Font = UiTheme.FuenteSubtitulo,
                TextAlign = ContentAlignment.MiddleLeft
            });

            var flowTarjetas = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                BackColor = UiTheme.Fondo,
                Padding = new Padding(4, 8, 16, 16)
            };

            AgregarTarjeta(flowTarjetas, "Socios", "Registrar socios, consultar listado y buscar por DNI.", "Gestión", () => AbrirFormulario<FormSocios>());
            AgregarTarjeta(flowTarjetas, "Visitantes", "Ingreso diario con registro de pago.", "Gestión", () => AbrirFormulario<FormVisitantes>());
            AgregarTarjeta(flowTarjetas, "Cobrar cuota", "Buscar socio, ver cuotas y registrar pagos.", "Cuotas", () => AbrirFormulario<FormCobroCuota>());
            AgregarTarjeta(flowTarjetas, "Carnets", "Consultar y renovar carnet de socio por DNI.", "Gestión", () => AbrirFormulario<FormCarnets>());
            AgregarTarjeta(flowTarjetas, "Confeccionar rutina", "Crear rutina según ficha médica del socio.", "Profesores", () => AbrirFormulario<FormRutinas>());
            AgregarTarjeta(flowTarjetas, "Firmar asistencia", "Registrar y firmar asistencia de profesores.", "Profesores", () => AbrirFormulario<FormAsistencias>());
            AgregarTarjeta(flowTarjetas, "Turnos nutrición", "Asignar turnos y actualizar ficha médica en consulta.", "Nutrición", () => AbrirFormulario<FormTurnosNutricion>());
            AgregarTarjeta(flowTarjetas, "Reportes", "Cuotas por vencer y vencidas para gestión de cobranza.", "Reportes", () => AbrirFormulario<FormReportes>());

            contenedor.Controls.Add(flowTarjetas);
            contenedor.Controls.Add(panelInfo);
            contenedor.Controls.Add(panelHeader);

            return contenedor;
        }

        private static void AgregarTarjeta(FlowLayoutPanel contenedor, string titulo, string descripcion, string modulo, Action abrir)
        {
            contenedor.Controls.Add(UiTheme.CrearTarjetaAcceso(titulo, descripcion, modulo, abrir));
        }

        private static void AbrirFormulario<T>() where T : Form, new()
        {
            using var form = new T();
            form.ShowDialog();
        }
    }
}

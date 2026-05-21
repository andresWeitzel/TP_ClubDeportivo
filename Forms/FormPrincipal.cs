using System.Drawing;
using System.Windows.Forms;

namespace TP_ClubDeportivo.Forms
{
    public class FormPrincipal : Form
    {
        private readonly Panel panelContenido;
        private readonly Panel panelDashboard;
        private readonly ToolStripStatusLabel lblStatus;
        private readonly Label lblSubtituloPagina;

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

            lblSubtituloPagina = new Label
            {
                Text = $"Sesión: {Sesion.UsuarioActual?.Username} · Rol: {Sesion.UsuarioActual?.Rol}",
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
            lblStatus = new ToolStripStatusLabel(ObtenerTextoEstado())
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

        private static string ObtenerTextoEstado()
        {
            var rol = Sesion.UsuarioActual?.Rol ?? "";
            var modulos = ContarModulosVisibles();
            return $"Conectado · {rol} · {modulos} módulo(s) disponible(s)";
        }

        private static int ContarModulosVisibles()
        {
            var total = 0;
            foreach (Permisos.Modulo modulo in Enum.GetValues(typeof(Permisos.Modulo)))
            {
                if (modulo is Permisos.Modulo.ReporteAsistenciaProfesores
                    or Permisos.Modulo.ControlVencimientoCuotas)
                {
                    continue;
                }

                if (Permisos.PuedeAcceder(modulo))
                {
                    total++;
                }
            }

            return total;
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

            AgregarBotonSidebar(panelNav, Permisos.Modulo.Reportes, "Reportes", () => AbrirFormulario<FormReportes>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.LiquidarHaberes, "Liquidar haberes", () => AbrirFormulario<FormLiquidarHaberes>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.TurnosNutricion, "Turnos nutrición", () => AbrirFormulario<FormTurnosNutricion>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.Rutinas, "Confeccionar rutina", () => AbrirFormulario<FormRutinas>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.FirmarAsistencia, "Firmar asistencia", () => AbrirFormulario<FormAsistencias>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.Carnets, "Carnets", () => AbrirFormulario<FormCarnets>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.CobroCuota, "Cobrar cuota", () => AbrirFormulario<FormCobroCuota>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.Visitantes, "Visitantes", () => AbrirFormulario<FormVisitantes>());
            AgregarBotonSidebar(panelNav, Permisos.Modulo.Socios, "Socios", () => AbrirFormulario<FormSocios>());

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

        private static void AgregarBotonSidebar(Panel panelNav, Permisos.Modulo modulo, string texto, Action abrir)
        {
            if (!Permisos.PuedeAcceder(modulo))
            {
                return;
            }

            var btn = UiTheme.CrearBotonSidebar(texto, (_, _) => abrir());
            panelNav.Controls.Add(btn);
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
                Text = ObtenerSubtituloDashboard(),
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
                Text = ObtenerTipDashboard(),
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

            AgregarTarjeta(flowTarjetas, Permisos.Modulo.Socios, "Socios", "Registrar socios, consultar listado y buscar por DNI.", "Gestión", () => AbrirFormulario<FormSocios>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.Visitantes, "Visitantes", "Ingreso diario con registro de pago.", "Gestión", () => AbrirFormulario<FormVisitantes>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.CobroCuota, "Cobrar cuota", "Buscar socio, ver cuotas y registrar pagos.", "Cuotas", () => AbrirFormulario<FormCobroCuota>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.Carnets, "Carnets", "Consultar y renovar carnet de socio por DNI.", "Gestión", () => AbrirFormulario<FormCarnets>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.Rutinas, "Confeccionar rutina", "Crear rutina según ficha médica del socio.", "Profesores", () => AbrirFormulario<FormRutinas>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.FirmarAsistencia, "Firmar asistencia", "Registrar y firmar asistencia de profesores.", "Profesores", () => AbrirFormulario<FormAsistencias>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.TurnosNutricion, "Turnos nutrición", "Asignar turnos y actualizar ficha médica en consulta.", "Nutrición", () => AbrirFormulario<FormTurnosNutricion>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.Reportes, "Reportes", "Cuotas por vencer, morosos y asistencia de profesores.", "Reportes", () => AbrirFormulario<FormReportes>());
            AgregarTarjeta(flowTarjetas, Permisos.Modulo.LiquidarHaberes, "Liquidar haberes", "Calcular liquidaciones, emitir recibos y pagar a profesores.", "Personal", () => AbrirFormulario<FormLiquidarHaberes>());

            if (flowTarjetas.Controls.Count == 0)
            {
                flowTarjetas.Controls.Add(new Label
                {
                    Text = "No hay módulos habilitados para su rol.",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F, FontStyle.Italic),
                    ForeColor = UiTheme.TextoSecundario,
                    Padding = new Padding(8)
                });
            }

            contenedor.Controls.Add(flowTarjetas);
            contenedor.Controls.Add(panelInfo);
            contenedor.Controls.Add(panelHeader);

            return contenedor;
        }

        private static string ObtenerSubtituloDashboard()
        {
            if (Sesion.TieneRol(Permisos.Administrador))
            {
                return "Acceso completo — todos los módulos del club.";
            }

            if (Sesion.TieneRol(Permisos.Empleado))
            {
                return "Recepción y cobranza — socios, visitantes, cuotas y reportes.";
            }

            if (Sesion.TieneRol(Permisos.Profesor))
            {
                return "Área deportiva — asistencia y rutinas personalizadas.";
            }

            if (Sesion.TieneRol(Permisos.Nutricionista))
            {
                return "Área de salud — turnos y fichas médicas en consulta.";
            }

            return "Elegí un módulo del menú lateral.";
        }

        private static string ObtenerTipDashboard()
        {
            if (Sesion.TieneRol(Permisos.Administrador))
            {
                return "Administrador: acceso a gestión, cobranza, personal, nutrición y reportes.";
            }

            if (Sesion.TieneRol(Permisos.Empleado))
            {
                return "Empleado: socios, visitantes, carnets, cobro de cuotas y reportes de mora.";
            }

            if (Sesion.TieneRol(Permisos.Profesor))
            {
                return "Profesor: solo firmar asistencia y confeccionar rutinas.";
            }

            if (Sesion.TieneRol(Permisos.Nutricionista))
            {
                return "Nutricionista: solo gestión de turnos de nutrición.";
            }

            return "Los módulos visibles dependen de su rol en el sistema.";
        }

        private static void AgregarTarjeta(FlowLayoutPanel contenedor, Permisos.Modulo modulo, string titulo, string descripcion, string categoria, Action abrir)
        {
            if (!Permisos.PuedeAcceder(modulo))
            {
                return;
            }

            contenedor.Controls.Add(UiTheme.CrearTarjetaAcceso(titulo, descripcion, categoria, abrir));
        }

        private static void AbrirFormulario<T>() where T : Form, new()
        {
            if (!Permisos.IntentarAbrirFormulario<T>(out var mensaje))
            {
                MessageBox.Show(mensaje, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new T();
            form.ShowDialog();
        }
    }
}

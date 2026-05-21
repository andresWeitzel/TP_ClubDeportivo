using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormReportes : Form
    {
        private readonly TabControl tabReportes;
        private readonly DataGridView dgvPorVencer;
        private readonly DataGridView dgvVencidas;
        private readonly NumericUpDown numDias;
        private readonly Label lblResumenPorVencer;
        private readonly Label lblResumenVencidas;
        private readonly Button btnGenerarPorVencer;
        private readonly Button btnGenerarVencidas;
        private readonly Button btnCobrarSeleccion;
        private readonly Button btnControlVencimiento;
        private readonly Label lblControlCu04;
        private readonly DataGridView dgvAsistenciaProfesores;
        private readonly DateTimePicker dtpAsistenciaDesde;
        private readonly DateTimePicker dtpAsistenciaHasta;
        private readonly ComboBox cboProfesorAsistencia;
        private readonly Label lblResumenAsistencia;
        private readonly Button btnGenerarAsistencia;
        private readonly Button btnExportar;
        private readonly TabPage tabAsistencia;
        private readonly Panel panelControlCu04;

        private readonly ReporteDAO _reporteDao = new();
        private readonly ProfesorDAO _profesorDao = new();

        public FormReportes()
        {
            Text = "Generar reportes (CU-09)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 640);
            MinimumSize = new Size(920, 560);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            tabReportes = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Point(12, 8)
            };

            var tabPorVencer = new TabPage("Cuotas por vencer")
            {
                BackColor = UiTheme.Fondo,
                Padding = new Padding(12)
            };
            var tabVencidas = new TabPage("Morosos")
            {
                BackColor = UiTheme.Fondo,
                Padding = new Padding(12)
            };
            tabAsistencia = new TabPage("Asistencia profesores")
            {
                BackColor = UiTheme.Fondo,
                Padding = new Padding(12)
            };

            // --- Tab por vencer ---
            var panelFiltroPorVencer = CrearPanelFiltro();

            var flowPorVencer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(4, 6, 4, 6),
                BackColor = UiTheme.Tarjeta
            };

            flowPorVencer.Controls.Add(new Label
            {
                Text = "Vencen en los próximos",
                AutoSize = true,
                Margin = new Padding(8, 10, 0, 0),
                Font = UiTheme.FuenteNormal
            });

            numDias = new NumericUpDown
            {
                Width = 64,
                Minimum = 1,
                Maximum = 90,
                Value = 7,
                Margin = new Padding(8, 6, 0, 0)
            };

            flowPorVencer.Controls.Add(numDias);
            flowPorVencer.Controls.Add(new Label
            {
                Text = "días",
                AutoSize = true,
                Margin = new Padding(8, 10, 0, 0),
                Font = UiTheme.FuenteNormal
            });

            btnGenerarPorVencer = new Button
            {
                Text = "Generar reporte",
                AutoSize = true,
                MinimumSize = new Size(130, 32),
                Margin = new Padding(16, 4, 0, 0)
            };
            UiTheme.AplicarBotonPrimario(btnGenerarPorVencer);
            btnGenerarPorVencer.Click += (_, _) => CargarCuotasPorVencer();
            flowPorVencer.Controls.Add(btnGenerarPorVencer);

            panelFiltroPorVencer.Controls.Add(flowPorVencer);

            lblResumenPorVencer = CrearLabelResumen(UiTheme.PrimarioClaro, UiTheme.PrimarioOscuro);

            dgvPorVencer = CrearGrilla();
            dgvPorVencer.DataBindingComplete += (_, _) => ConfigurarColumnasPorVencer();
            dgvPorVencer.CellDoubleClick += (_, _) => AbrirCobroDesdeGrilla(dgvPorVencer);

            var panelGrillaPorVencer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            panelGrillaPorVencer.Controls.Add(dgvPorVencer);

            tabPorVencer.Controls.Add(panelGrillaPorVencer);
            tabPorVencer.Controls.Add(lblResumenPorVencer);
            tabPorVencer.Controls.Add(panelFiltroPorVencer);

            // --- Tab vencidas ---
            var panelFiltroVencidas = CrearPanelFiltro();

            var panelFiltroVencidasInner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(8, 6, 8, 6)
            };
            panelFiltroVencidasInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panelFiltroVencidasInner.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            panelFiltroVencidasInner.Controls.Add(new Label
            {
                Text = "Cuotas con vencimiento anterior a hoy y sin pago registrado.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.TextoSecundario
            }, 0, 0);

            btnGenerarVencidas = new Button
            {
                Text = "Generar reporte",
                AutoSize = true,
                MinimumSize = new Size(130, 32),
                Margin = new Padding(8, 0, 0, 0)
            };
            UiTheme.AplicarBotonPrimario(btnGenerarVencidas);
            btnGenerarVencidas.Click += (_, _) => CargarCuotasVencidas();
            panelFiltroVencidasInner.Controls.Add(btnGenerarVencidas, 1, 0);

            panelFiltroVencidas.Controls.Add(panelFiltroVencidasInner);

            lblResumenVencidas = CrearLabelResumen(
                Color.FromArgb(255, 235, 235),
                UiTheme.Error);

            dgvVencidas = CrearGrilla();
            dgvVencidas.DataBindingComplete += (_, _) => ConfigurarColumnasVencidas();
            dgvVencidas.CellDoubleClick += (_, _) => AbrirCobroDesdeGrilla(dgvVencidas);

            var panelGrillaVencidas = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            panelGrillaVencidas.Controls.Add(dgvVencidas);

            tabVencidas.Controls.Add(panelGrillaVencidas);
            tabVencidas.Controls.Add(lblResumenVencidas);
            tabVencidas.Controls.Add(panelFiltroVencidas);

            // --- Tab asistencia profesores (RF-17) ---
            var panelFiltroAsistencia = CrearPanelFiltro();

            var flowAsistencia = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(4, 6, 4, 6),
                BackColor = UiTheme.Tarjeta
            };

            flowAsistencia.Controls.Add(new Label
            {
                Text = "Desde:",
                AutoSize = true,
                Margin = new Padding(8, 10, 0, 0),
                Font = UiTheme.FuenteNormal
            });

            dtpAsistenciaDesde = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(6, 6, 0, 0),
                Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            };

            flowAsistencia.Controls.Add(dtpAsistenciaDesde);
            flowAsistencia.Controls.Add(new Label
            {
                Text = "Hasta:",
                AutoSize = true,
                Margin = new Padding(12, 10, 0, 0),
                Font = UiTheme.FuenteNormal
            });

            dtpAsistenciaHasta = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 110,
                Margin = new Padding(6, 6, 0, 0),
                Value = DateTime.Today
            };

            flowAsistencia.Controls.Add(dtpAsistenciaHasta);
            flowAsistencia.Controls.Add(new Label
            {
                Text = "Profesor:",
                AutoSize = true,
                Margin = new Padding(12, 10, 0, 0),
                Font = UiTheme.FuenteNormal
            });

            cboProfesorAsistencia = new ComboBox
            {
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Margin = new Padding(6, 6, 0, 0),
                Font = UiTheme.FuenteNormal
            };

            flowAsistencia.Controls.Add(cboProfesorAsistencia);

            btnGenerarAsistencia = new Button
            {
                Text = "Generar reporte",
                AutoSize = true,
                MinimumSize = new Size(130, 32),
                Margin = new Padding(16, 4, 0, 0)
            };
            UiTheme.AplicarBotonPrimario(btnGenerarAsistencia);
            btnGenerarAsistencia.Click += (_, _) => CargarAsistenciaProfesores();
            flowAsistencia.Controls.Add(btnGenerarAsistencia);

            panelFiltroAsistencia.Controls.Add(flowAsistencia);

            lblResumenAsistencia = CrearLabelResumen(UiTheme.PrimarioClaro, UiTheme.PrimarioOscuro);

            dgvAsistenciaProfesores = CrearGrilla();
            dgvAsistenciaProfesores.DataBindingComplete += (_, _) => ConfigurarColumnasAsistencia();

            var panelGrillaAsistencia = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };
            panelGrillaAsistencia.Controls.Add(dgvAsistenciaProfesores);

            tabAsistencia.Controls.Add(panelGrillaAsistencia);
            tabAsistencia.Controls.Add(lblResumenAsistencia);
            tabAsistencia.Controls.Add(panelFiltroAsistencia);

            tabReportes.TabPages.Add(tabPorVencer);
            tabReportes.TabPages.Add(tabVencidas);
            tabReportes.TabPages.Add(tabAsistencia);
            tabReportes.SelectedIndexChanged += (_, _) =>
            {
                ActualizarBotonCobrar();
                ActualizarBotonExportar();
            };

            btnCobrarSeleccion = new Button
            {
                Text = "Cobrar cuota del socio seleccionado",
                AutoSize = false,
                Size = new Size(320, 42),
                Margin = new Padding(0, 0, 12, 0)
            };
            UiTheme.AplicarBotonPrimario(btnCobrarSeleccion);
            btnCobrarSeleccion.Click += (_, _) => CobrarSocioSeleccionado();
            btnCobrarSeleccion.Enabled = false;

            btnExportar = new Button
            {
                Text = "Exportar a CSV",
                AutoSize = false,
                Size = new Size(200, 42),
                Margin = new Padding(0, 0, 12, 0)
            };
            UiTheme.AplicarBotonExito(btnExportar);
            btnExportar.Click += (_, _) => ExportarReporteActual();

            var flowAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(4, 6, 4, 6),
                BackColor = UiTheme.Tarjeta
            };
            flowAcciones.Controls.Add(btnExportar);
            flowAcciones.Controls.Add(btnCobrarSeleccion);

            var panelAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(16, 6, 16, 6)
            };
            panelAcciones.Paint += (_, e) =>
            {
                using var pen = new Pen(UiTheme.Borde);
                e.Graphics.DrawLine(pen, 0, 0, panelAcciones.Width, 0);
            };
            panelAcciones.Controls.Add(flowAcciones);

            var lblAyuda = new Label
            {
                Text = "CU-09: elegí el tipo de reporte en las pestañas, generá el listado y exportalo si lo necesitás. En cuotas: doble clic para cobrar.",
                Dock = DockStyle.Bottom,
                Height = 28,
                ForeColor = UiTheme.TextoSecundario,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                BackColor = UiTheme.Fondo
            };

            var panelEncabezadoCu09 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(20, 12, 20, 8)
            };
            panelEncabezadoCu09.Paint += (_, e) =>
            {
                using var pen = new Pen(UiTheme.Borde);
                e.Graphics.DrawLine(pen, 0, panelEncabezadoCu09.Height - 1, panelEncabezadoCu09.Width, panelEncabezadoCu09.Height - 1);
            };

            panelEncabezadoCu09.Controls.Add(new Label
            {
                Text = "Generar reportes",
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = UiTheme.Texto
            });
            panelEncabezadoCu09.Controls.Add(new Label
            {
                Text = "Seleccioná el tipo de reporte: cuotas por vencer, morosos o asistencia de profesores.",
                Dock = DockStyle.Fill,
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario
            });

            panelControlCu04 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = UiTheme.PrimarioClaro,
                Padding = new Padding(12, 10, 12, 8)
            };

            lblControlCu04 = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.PrimarioOscuro,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "CU-04: al abrir se evalúan vencimientos y se actualizan mora / socios suspendidos."
            };

            btnControlVencimiento = new Button
            {
                Text = "Ejecutar control hoy",
                Dock = DockStyle.Right,
                Width = 170,
                Height = 34
            };
            UiTheme.AplicarBotonPrimario(btnControlVencimiento);
            btnControlVencimiento.Click += (_, _) => EjecutarControlVencimientoCu04(mostrarResumen: true);

            panelControlCu04.Controls.Add(lblControlCu04);
            panelControlCu04.Controls.Add(btnControlVencimiento);

            Controls.Add(tabReportes);
            Controls.Add(panelAcciones);
            Controls.Add(lblAyuda);
            Controls.Add(panelControlCu04);
            Controls.Add(panelEncabezadoCu09);

            dgvPorVencer.SelectionChanged += (_, _) => ActualizarBotonCobrar();
            dgvVencidas.SelectionChanged += (_, _) => ActualizarBotonCobrar();

            Load += FormReportes_Load;
        }

        private void FormReportes_Load(object? sender, EventArgs e)
        {
            if (!Permisos.IntentarAcceder(Permisos.Modulo.Reportes, out var mensaje))
            {
                MessageBox.Show(mensaje, "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            AplicarPermisosPorRol();

            if (Permisos.PuedeAcceder(Permisos.Modulo.ReporteAsistenciaProfesores))
            {
                CargarProfesoresFiltroAsistencia();
            }

            if (Permisos.PuedeAcceder(Permisos.Modulo.ControlVencimientoCuotas))
            {
                EjecutarControlVencimientoCu04(mostrarResumen: false);
            }
            else
            {
                CargarCuotasPorVencer();
                CargarCuotasVencidas();
            }

            ActualizarBotonExportar();
        }

        private void AplicarPermisosPorRol()
        {
            if (!Permisos.PuedeAcceder(Permisos.Modulo.ReporteAsistenciaProfesores)
                && tabReportes.TabPages.Contains(tabAsistencia))
            {
                tabReportes.TabPages.Remove(tabAsistencia);
            }

            panelControlCu04.Visible = Permisos.PuedeAcceder(Permisos.Modulo.ControlVencimientoCuotas);

            Text = Sesion.TieneRol(Permisos.Administrador)
                ? "Generar reportes (CU-09)"
                : "Reportes de cobranza (CU-09)";
        }

        private void CargarProfesoresFiltroAsistencia()
        {
            try
            {
                cboProfesorAsistencia.Items.Clear();
                cboProfesorAsistencia.Items.Add(new ProfesorFiltroItem(0, "Todos los profesores"));

                foreach (var profesor in _profesorDao.ObtenerTodos())
                {
                    cboProfesorAsistencia.Items.Add(
                        new ProfesorFiltroItem(profesor.IdProfesor, $"{profesor.Nombre} {profesor.Apellido}"));
                }

                cboProfesorAsistencia.SelectedIndex = 0;
            }
            catch
            {
                cboProfesorAsistencia.Items.Clear();
                cboProfesorAsistencia.Items.Add(new ProfesorFiltroItem(0, "Todos los profesores"));
                cboProfesorAsistencia.SelectedIndex = 0;
            }
        }

        private void CargarAsistenciaProfesores()
        {
            if (dtpAsistenciaDesde.Value.Date > dtpAsistenciaHasta.Value.Date)
            {
                MessageBox.Show(
                    "La fecha «Desde» no puede ser posterior a «Hasta».",
                    "Asistencia profesores",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var profesorId = cboProfesorAsistencia.SelectedItem is ProfesorFiltroItem item
                    ? item.IdProfesor
                    : 0;

                var lista = _reporteDao
                    .ObtenerAsistenciaProfesores(dtpAsistenciaDesde.Value.Date, dtpAsistenciaHasta.Value.Date, profesorId)
                    .ToList();

                dgvAsistenciaProfesores.DataSource = lista;

                var desde = dtpAsistenciaDesde.Value.ToString("dd/MM/yyyy");
                var hasta = dtpAsistenciaHasta.Value.ToString("dd/MM/yyyy");
                var filtroProfesor = cboProfesorAsistencia.Text;

                if (lista.Count == 0)
                {
                    lblResumenAsistencia.Text =
                        $"Período {desde} — {hasta} · {filtroProfesor}: sin registros de asistencia.";
                }
                else
                {
                    var promedio = lista.Average(r => r.PorcentajeAsistencia);
                    lblResumenAsistencia.Text =
                        $"Período {desde} — {hasta} · {filtroProfesor} — " +
                        $"{lista.Count} profesor(es) · Promedio asistencia: {promedio:N1}%";
                }

                ActualizarBotonExportar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarReporteActual()
        {
            var (grilla, titulo) = ObtenerReporteActivo();
            ExportadorCsv.ExportarGrilla(grilla, titulo);
        }

        private (DataGridView Grilla, string Titulo) ObtenerReporteActivo()
        {
            return tabReportes.SelectedIndex switch
            {
                0 => (dgvPorVencer, "cuotas_por_vencer"),
                1 => (dgvVencidas, "morosos"),
                2 => (dgvAsistenciaProfesores, "asistencia_profesores"),
                _ => (dgvPorVencer, "reporte")
            };
        }

        private void ActualizarBotonExportar()
        {
            var grilla = ObtenerReporteActivo().Grilla;
            btnExportar.Enabled = grilla.Rows.Count > 0;
        }

        /// <summary>
        /// CU-04: evalúa cuotas vencidas, marca mora y suspende socios; luego refresca listados.
        /// </summary>
        private void EjecutarControlVencimientoCu04(bool mostrarResumen)
        {
            try
            {
                var resultado = _reporteDao.EjecutarControlVencimiento();
                if (resultado is null)
                {
                    lblControlCu04.Text = "CU-04: no se pudo ejecutar el control (verifique sp_controlar_vencimiento_cuotas).";
                    lblControlCu04.ForeColor = UiTheme.Error;
                    return;
                }

                lblControlCu04.ForeColor = UiTheme.PrimarioOscuro;
                lblControlCu04.Text =
                    $"CU-04 ejecutado ({DateTime.Now:dd/MM/yyyy HH:mm}) — " +
                    $"{resultado.CuotasEnMora} cuota(s) en mora, " +
                    $"{resultado.SociosSuspendidos} socio(s) suspendido(s) (estado MORA).";

                CargarCuotasPorVencer();
                CargarCuotasVencidas();

                if (mostrarResumen)
                {
                    MessageBox.Show(
                        $"Control de vencimiento completado.\n\n" +
                        $"• Cuotas en mora: {resultado.CuotasEnMora}\n" +
                        $"• Socios suspendidos (MORA): {resultado.SociosSuspendidos}\n\n" +
                        "Los listados «Próximos a vencer» y «En mora / vencidas» fueron actualizados.",
                        "CU-04 — Control de vencimiento",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en control de vencimiento: {ex.Message}", "CU-04", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Panel CrearPanelFiltro()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = UiTheme.Tarjeta
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(UiTheme.Borde);
                e.Graphics.DrawLine(pen, 0, panel.Height - 1, panel.Width, panel.Height - 1);
            };
            return panel;
        }

        private static Label CrearLabelResumen(Color fondo, Color texto)
        {
            return new Label
            {
                Dock = DockStyle.Bottom,
                Height = 36,
                Padding = new Padding(12, 8, 12, 0),
                ForeColor = texto,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                BackColor = fondo,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private static DataGridView CrearGrilla()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                MultiSelect = false
            };
        }

        private void ActualizarBotonCobrar()
        {
            var esTabCuotas = tabReportes.SelectedIndex is 0 or 1;
            btnCobrarSeleccion.Visible = esTabCuotas;

            if (!esTabCuotas)
            {
                btnCobrarSeleccion.Enabled = false;
                return;
            }

            var grilla = tabReportes.SelectedIndex == 0 ? dgvPorVencer : dgvVencidas;
            btnCobrarSeleccion.Enabled = grilla.CurrentRow?.DataBoundItem is CuotaReporte;
        }

        private void CobrarSocioSeleccionado()
        {
            var grilla = tabReportes.SelectedIndex == 0 ? dgvPorVencer : dgvVencidas;
            AbrirCobroDesdeGrilla(grilla);
        }

        private void AbrirCobroDesdeGrilla(DataGridView grilla)
        {
            if (grilla.CurrentRow?.DataBoundItem is not CuotaReporte fila)
            {
                MessageBox.Show("Seleccione un socio de la grilla.", "Cobrar cuota", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormCobroCuota(fila.Dni);
            form.ShowDialog();
            RefrescarReportes();
        }

        private void RefrescarReportes()
        {
            EjecutarControlVencimientoCu04(mostrarResumen: false);
        }

        private void CargarCuotasPorVencer()
        {
            try
            {
                var dias = (int)numDias.Value;
                var lista = _reporteDao.ObtenerCuotasPorVencer(dias).ToList();

                if (!ValidarReportePorVencer(lista, dias, out var advertencia))
                {
                    lblResumenPorVencer.ForeColor = UiTheme.Error;
                    lblResumenPorVencer.Text = advertencia;
                    dgvPorVencer.DataSource = null;
                    return;
                }

                dgvPorVencer.DataSource = lista;
                lblResumenPorVencer.ForeColor = UiTheme.PrimarioOscuro;

                var totalMonto = lista.Sum(c => c.Monto);
                lblResumenPorVencer.Text = lista.Count == 0
                    ? $"No hay cuotas por vencer en los próximos {dias} días."
                    : $"Total: {lista.Count} cuota(s) por vencer — Monto acumulado: {FormatearMonto(totalMonto)}";

                ActualizarBotonExportar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarCuotasVencidas()
        {
            try
            {
                var lista = _reporteDao.ObtenerCuotasVencidas().ToList();

                if (!ValidarReporteVencidas(lista, out var advertencia))
                {
                    lblResumenVencidas.Text = advertencia;
                    dgvVencidas.DataSource = null;
                    return;
                }

                dgvVencidas.DataSource = lista;

                var totalMonto = lista.Sum(c => c.Monto);
                lblResumenVencidas.Text = lista.Count == 0
                    ? "No hay cuotas vencidas pendientes de cobro."
                    : $"Total: {lista.Count} cuota(s) vencida(s) — Deuda acumulada: {FormatearMonto(totalMonto)}";

                AplicarEstiloFilasVencidas();
                ActualizarBotonExportar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasAsistencia()
        {
            if (dgvAsistenciaProfesores.Columns.Count == 0)
            {
                return;
            }

            OcultarColumna(dgvAsistenciaProfesores, "IdProfesor");

            AplicarColumnasComunes(dgvAsistenciaProfesores, new Dictionary<string, string>
            {
                ["ProfesorNombre"] = "Profesor",
                ["Especialidad"] = "Especialidad",
                ["TotalRegistros"] = "Registros",
                ["Asistencias"] = "Presentes",
                ["Inasistencias"] = "Ausentes",
                ["PorcentajeAsistencia"] = "% asistencia"
            });

            if (dgvAsistenciaProfesores.Columns.Contains("PorcentajeAsistencia"))
            {
                dgvAsistenciaProfesores.Columns["PorcentajeAsistencia"]!.DefaultCellStyle.Format = "N1";
            }
        }

        /// <summary>Valida coherencia RF-15: vencimiento futuro, días restantes >= 0.</summary>
        private static bool ValidarReportePorVencer(List<CuotaReporte> lista, int dias, out string advertencia)
        {
            advertencia = string.Empty;
            var hoy = DateTime.Today;

            foreach (var item in lista)
            {
                if (item.FechaVencimiento.Date < hoy)
                {
                    advertencia = $"Validación RF-15: la cuota #{item.IdCuota} tiene vencimiento pasado ({item.FechaVencimiento:dd/MM/yyyy}).";
                    return false;
                }

                if (item.Dias < 0 || item.Dias > dias)
                {
                    advertencia = $"Validación RF-15: días restantes inconsistentes en cuota #{item.IdCuota} (valor: {item.Dias}, rango esperado: 0–{dias}).";
                    return false;
                }
            }

            return true;
        }

        /// <summary>Valida coherencia RF-16: vencimiento pasado, días de atraso > 0.</summary>
        private static bool ValidarReporteVencidas(List<CuotaReporte> lista, out string advertencia)
        {
            advertencia = string.Empty;
            var hoy = DateTime.Today;

            foreach (var item in lista)
            {
                if (item.FechaVencimiento.Date >= hoy)
                {
                    advertencia = $"Validación RF-16: la cuota #{item.IdCuota} no está vencida (vence {item.FechaVencimiento:dd/MM/yyyy}).";
                    return false;
                }

                if (item.Dias <= 0)
                {
                    advertencia = $"Validación RF-16: días de atraso inválidos en cuota #{item.IdCuota} (valor: {item.Dias}).";
                    return false;
                }
            }

            return true;
        }

        private void ConfigurarColumnasPorVencer()
        {
            if (dgvPorVencer.Columns.Count == 0)
            {
                return;
            }

            AplicarColumnasComunes(dgvPorVencer, new Dictionary<string, string>
            {
                ["IdSocio"] = "Nº socio",
                ["Dni"] = "DNI",
                ["NombreCompleto"] = "Socio",
                ["EstadoCuotaSocio"] = "Estado socio",
                ["IdCuota"] = "Nº cuota",
                ["Monto"] = "Monto ($)",
                ["FechaVencimiento"] = "Vencimiento",
                ["Dias"] = "Días restantes"
            });

            OcultarColumna(dgvPorVencer, "Nombre");
            OcultarColumna(dgvPorVencer, "Apellido");
        }

        private void ConfigurarColumnasVencidas()
        {
            if (dgvVencidas.Columns.Count == 0)
            {
                return;
            }

            AplicarColumnasComunes(dgvVencidas, new Dictionary<string, string>
            {
                ["IdSocio"] = "Nº socio",
                ["Dni"] = "DNI",
                ["NombreCompleto"] = "Socio",
                ["EstadoCuotaSocio"] = "Estado socio",
                ["IdCuota"] = "Nº cuota",
                ["Monto"] = "Monto ($)",
                ["FechaVencimiento"] = "Vencimiento",
                ["Dias"] = "Días de atraso"
            });

            OcultarColumna(dgvVencidas, "Nombre");
            OcultarColumna(dgvVencidas, "Apellido");
        }

        private static void AplicarColumnasComunes(DataGridView grilla, Dictionary<string, string> columnas)
        {
            foreach (var par in columnas)
            {
                if (grilla.Columns.Contains(par.Key))
                {
                    grilla.Columns[par.Key]!.HeaderText = par.Value;
                }
            }

            if (grilla.Columns.Contains("FechaVencimiento"))
            {
                grilla.Columns["FechaVencimiento"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (grilla.Columns.Contains("Monto"))
            {
                grilla.Columns["Monto"]!.DefaultCellStyle.Format = "N2";
                grilla.Columns["Monto"]!.DefaultCellStyle.FormatProvider = CultureInfo.CurrentCulture;
            }

            int orden = 0;
            foreach (var clave in columnas.Keys)
            {
                if (grilla.Columns.Contains(clave))
                {
                    grilla.Columns[clave]!.DisplayIndex = orden++;
                }
            }
        }

        private void AplicarEstiloFilasVencidas()
        {
            foreach (DataGridViewRow row in dgvVencidas.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                row.DefaultCellStyle.ForeColor = UiTheme.Error;
            }
        }

        private static void OcultarColumna(DataGridView grilla, string nombre)
        {
            if (grilla.Columns.Contains(nombre))
            {
                grilla.Columns[nombre]!.Visible = false;
            }
        }

        private static string FormatearMonto(decimal monto) =>
            monto.ToString("C2", CultureInfo.CurrentCulture);

        private sealed class ProfesorFiltroItem(int idProfesor, string texto)
        {
            public int IdProfesor { get; } = idProfesor;

            public override string ToString() => texto;
        }
    }
}

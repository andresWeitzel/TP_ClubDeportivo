using System.Drawing;
using System.Globalization;
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

        private readonly ReporteDAO _reporteDao = new();

        public FormReportes()
        {
            Text = "Control de vencimiento y reportes (CU-04)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 600);
            MinimumSize = new Size(900, 520);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            tabReportes = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Padding = new Point(12, 8)
            };

            var tabPorVencer = new TabPage("Próximos a vencer")
            {
                BackColor = UiTheme.Fondo,
                Padding = new Padding(12)
            };
            var tabVencidas = new TabPage("En mora / vencidas")
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
                Text = "Actualizar",
                AutoSize = true,
                MinimumSize = new Size(110, 32),
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
                Text = "Actualizar",
                AutoSize = true,
                MinimumSize = new Size(110, 32),
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

            tabReportes.TabPages.Add(tabPorVencer);
            tabReportes.TabPages.Add(tabVencidas);
            tabReportes.SelectedIndexChanged += (_, _) => ActualizarBotonCobrar();

            btnCobrarSeleccion = new Button
            {
                Text = "Cobrar cuota del socio seleccionado",
                Dock = DockStyle.Bottom,
                Height = 44,
                Margin = new Padding(0)
            };
            UiTheme.AplicarBotonSecundario(btnCobrarSeleccion);
            btnCobrarSeleccion.Click += (_, _) => CobrarSocioSeleccionado();
            btnCobrarSeleccion.Enabled = false;

            var lblAyuda = new Label
            {
                Text = "Tip: doble clic en una fila o use el botón inferior para ir a Cobrar cuota.",
                Dock = DockStyle.Bottom,
                Height = 28,
                ForeColor = UiTheme.TextoSecundario,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0),
                BackColor = UiTheme.Fondo
            };

            var panelControlCu04 = new Panel
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
            Controls.Add(btnCobrarSeleccion);
            Controls.Add(lblAyuda);
            Controls.Add(panelControlCu04);

            dgvPorVencer.SelectionChanged += (_, _) => ActualizarBotonCobrar();
            dgvVencidas.SelectionChanged += (_, _) => ActualizarBotonCobrar();

            Load += (_, _) => EjecutarControlVencimientoCu04(mostrarResumen: false);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}

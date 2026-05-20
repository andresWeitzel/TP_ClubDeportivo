using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormCobroCuota : Form
    {
        private readonly TextBox txtDni;
        private readonly Label lblSocio;
        private readonly Label lblDetalleCuota;
        private readonly DataGridView dgvCuotas;
        private readonly NumericUpDown numMonto;
        private readonly ComboBox cboMedioPago;
        private readonly TextBox txtConcepto;
        private readonly Button btnBuscar, btnCobrar, btnLimpiar;
        private readonly CheckBox chkGenerarProxima;
        private readonly NumericUpDown numMontoProxima;
        private readonly Label lblMontoProxima;

        private readonly SocioDAO _socioDao = new();
        private readonly CuotaDAO _cuotaDao = new();
        private readonly PagoDAO _pagoDao = new();

        private Socio? _socioSeleccionado;
        private readonly string? _dniInicial;

        public FormCobroCuota()
            : this(null)
        {
        }

        public FormCobroCuota(string? dniInicial)
        {
            _dniInicial = dniInicial;
            Text = "Cobro de cuota mensual (CU-03)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(950, 620);
            MinimumSize = new Size(880, 560);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                Padding = new Padding(12, 10, 12, 8),
                BackColor = UiTheme.Tarjeta
            };
            panelBusqueda.Paint += (_, e) =>
            {
                using var pen = new Pen(UiTheme.Borde);
                e.Graphics.DrawLine(pen, 0, panelBusqueda.Height - 1, panelBusqueda.Width, panelBusqueda.Height - 1);
            };

            panelBusqueda.Controls.Add(new Label
            {
                Text = "DNI del socio:",
                Location = new Point(12, 14),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            txtDni = new TextBox
            {
                Location = new Point(110, 10),
                Width = 160,
                PlaceholderText = "Ej: 12345678"
            };
            UiTheme.AplicarCampo(txtDni);

            btnBuscar = new Button
            {
                Text = "Buscar",
                Location = new Point(280, 8),
                Size = new Size(100, 32)
            };
            UiTheme.AplicarBotonPrimario(btnBuscar);
            btnBuscar.Click += (_, _) => BuscarSocio();

            btnLimpiar = new Button
            {
                Text = "Limpiar",
                Location = new Point(390, 8),
                Size = new Size(90, 32)
            };
            UiTheme.AplicarBotonSecundario(btnLimpiar);
            btnLimpiar.Click += (_, _) => LimpiarBusqueda();

            lblSocio = new Label
            {
                Location = new Point(12, 48),
                Size = new Size(700, 22),
                ForeColor = UiTheme.Primario,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
            };

            lblDetalleCuota = new Label
            {
                Location = new Point(12, 68),
                Size = new Size(900, 18),
                ForeColor = UiTheme.TextoSecundario,
                Font = new Font("Segoe UI", 9F)
            };

            panelBusqueda.Controls.AddRange([txtDni, btnBuscar, btnLimpiar, lblSocio, lblDetalleCuota]);

            dgvCuotas = new DataGridView
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
            dgvCuotas.SelectionChanged += DgvCuotas_SelectionChanged;
            dgvCuotas.DataBindingComplete += (_, _) => AplicarEstilosFilas();

            var panelGrilla = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = UiTheme.Fondo
            };
            panelGrilla.Controls.Add(dgvCuotas);

            var panelPago = new GroupBox
            {
                Text = "Registrar pago (CU-03)",
                Dock = DockStyle.Bottom,
                Height = 200,
                Padding = new Padding(16, 20, 16, 12),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = UiTheme.Tarjeta
            };

            const int y1 = 36;
            const int y2 = 76;
            const int y3 = 116;

            panelPago.Controls.Add(new Label
            {
                Text = "Monto ($):",
                Location = new Point(16, y1 + 4),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });
            numMonto = new NumericUpDown
            {
                Location = new Point(100, y1),
                Width = 120,
                DecimalPlaces = 2,
                Maximum = 999999m,
                Minimum = 0.01m,
                Value = 150m,
                ThousandsSeparator = true
            };
            numMonto.ValueChanged += (_, _) => SincronizarMontoProxima();

            panelPago.Controls.Add(new Label
            {
                Text = "Medio de pago:",
                Location = new Point(240, y1 + 4),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });
            cboMedioPago = new ComboBox
            {
                Location = new Point(350, y1),
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FuenteNormal
            };
            cboMedioPago.Items.AddRange(["Efectivo", "Tarjeta Débito", "Tarjeta Crédito", "Transferencia"]);
            cboMedioPago.SelectedIndex = 0;

            btnCobrar = new Button
            {
                Text = "Registrar cobro",
                Location = new Point(530, y1 - 2),
                Size = new Size(160, 38)
            };
            UiTheme.AplicarBotonPrimario(btnCobrar);
            btnCobrar.Click += BtnCobrar_Click;

            panelPago.Controls.Add(new Label
            {
                Text = "Concepto:",
                Location = new Point(16, y2 + 4),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });
            txtConcepto = new TextBox
            {
                Location = new Point(100, y2),
                Size = new Size(590, 28),
                Text = "Cuota mensual"
            };
            UiTheme.AplicarCampo(txtConcepto);

            chkGenerarProxima = new CheckBox
            {
                Text = "Generar próxima cuota (+30 días)",
                Location = new Point(16, y3),
                AutoSize = true,
                Checked = true,
                Font = UiTheme.FuenteNormal
            };

            lblMontoProxima = new Label
            {
                Text = "Monto próxima ($):",
                Location = new Point(280, y3 + 2),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            };
            numMontoProxima = new NumericUpDown
            {
                Location = new Point(410, y3 - 2),
                Width = 120,
                DecimalPlaces = 2,
                Maximum = 999999m,
                Minimum = 0.01m,
                Value = 150m,
                ThousandsSeparator = true
            };

            chkGenerarProxima.CheckedChanged += (_, _) =>
            {
                lblMontoProxima.Visible = chkGenerarProxima.Checked;
                numMontoProxima.Visible = chkGenerarProxima.Checked;
                if (chkGenerarProxima.Checked)
                {
                    SincronizarMontoProxima();
                }
            };

            panelPago.Controls.AddRange([
                numMonto, cboMedioPago, btnCobrar,
                txtConcepto, chkGenerarProxima, lblMontoProxima, numMontoProxima
            ]);

            Controls.Add(panelGrilla);
            Controls.Add(panelPago);
            Controls.Add(panelBusqueda);

            AcceptButton = btnBuscar;
            txtDni.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BuscarSocio();
                }
            };

            Shown += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(_dniInicial))
                {
                    return;
                }

                txtDni.Text = _dniInicial.Trim();
                BuscarSocio();
            };
        }

        private void DgvCuotas_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvCuotas.CurrentRow?.DataBoundItem is not Cuota cuota)
            {
                lblDetalleCuota.Text = string.Empty;
                return;
            }

            var total = cuota.MontoTotalConRecargo();
            EstablecerMonto(numMonto, total);
            SincronizarMontoProxima();
            ActualizarDetalleCuota(cuota);

            if (cuota.RequiereRecargo() && cuota.ObtenerRecargo() > 0)
            {
                txtConcepto.Text = $"Cuota mensual + recargo mora (cuota #{cuota.IdCuota})";
            }
            else
            {
                txtConcepto.Text = "Cuota mensual";
            }
        }

        private void ActualizarDetalleCuota(Cuota cuota)
        {
            if (cuota.Estado == "PAGADA")
            {
                lblDetalleCuota.Text = $"Cuota #{cuota.IdCuota} ya pagada — vencía el {cuota.FechaVencimiento:dd/MM/yyyy}.";
                lblDetalleCuota.ForeColor = UiTheme.TextoSecundario;
                btnCobrar.Enabled = false;
                return;
            }

            btnCobrar.Enabled = true;
            var recargo = cuota.ObtenerRecargo();

            if (cuota.RequiereRecargo() && recargo > 0)
            {
                lblDetalleCuota.ForeColor = UiTheme.Error;
                lblDetalleCuota.Text =
                    $"E1 — Cuota #{cuota.IdCuota} en mora (venció el {cuota.FechaVencimiento:dd/MM/yyyy}). " +
                    $"Base {FormatearMonto(cuota.Monto)} + recargo {FormatearMonto(recargo)} = " +
                    $"total {FormatearMonto(cuota.MontoTotalConRecargo())}.";
            }
            else if (cuota.RequiereRecargo())
            {
                lblDetalleCuota.ForeColor = UiTheme.Error;
                lblDetalleCuota.Text = $"Cuota #{cuota.IdCuota} en mora — venció el {cuota.FechaVencimiento:dd/MM/yyyy}.";
            }
            else
            {
                lblDetalleCuota.ForeColor = UiTheme.TextoSecundario;
                lblDetalleCuota.Text = $"Cuota #{cuota.IdCuota} pendiente — vence el {cuota.FechaVencimiento:dd/MM/yyyy}.";
            }
        }

        private void BuscarSocio()
        {
            _socioSeleccionado = null;
            dgvCuotas.DataSource = null;
            lblSocio.Text = string.Empty;
            lblDetalleCuota.Text = string.Empty;
            btnCobrar.Enabled = false;

            var dni = txtDni.Text.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                MessageBox.Show("Ingrese un DNI.", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _socioSeleccionado = _socioDao.ObtenerPorDni(dni);
                if (_socioSeleccionado is null)
                {
                    MessageBox.Show("Socio no encontrado.", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                lblSocio.Text =
                    $"{_socioSeleccionado.Nombre} {_socioSeleccionado.Apellido} — Estado cuota: {_socioSeleccionado.EstadoCuota}";

                var cuotas = _cuotaDao.ObtenerPorSocio(_socioSeleccionado.IdSocio).ToList();
                dgvCuotas.DataSource = cuotas;
                ConfigurarColumnasGrilla();
                SeleccionarCuotaPendiente(cuotas);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SeleccionarCuotaPendiente(List<Cuota> cuotas)
        {
            var pendiente = cuotas
                .Where(c => c.Estado != "PAGADA")
                .OrderBy(c => c.FechaVencimiento)
                .FirstOrDefault();

            if (pendiente is null)
            {
                lblDetalleCuota.Text = "El socio no tiene cuotas pendientes de cobro.";
                lblDetalleCuota.ForeColor = UiTheme.TextoSecundario;
                btnCobrar.Enabled = false;
                return;
            }

            foreach (DataGridViewRow row in dgvCuotas.Rows)
            {
                if (row.DataBoundItem is Cuota c && c.IdCuota == pendiente.IdCuota)
                {
                    row.Selected = true;
                    dgvCuotas.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            if (dgvCuotas.Columns.Count == 0)
            {
                return;
            }

            OcultarColumna("SocioId");
            OcultarColumna("Periodo");
            OcultarColumna("TipoCuota");

            var columnas = new Dictionary<string, string>
            {
                ["IdCuota"] = "Nº cuota",
                ["Monto"] = "Monto ($)",
                ["FechaEmision"] = "Emisión",
                ["FechaVencimiento"] = "Vencimiento",
                ["Estado"] = "Estado",
                ["EnMora"] = "En mora"
            };

            foreach (var par in columnas)
            {
                if (dgvCuotas.Columns.Contains(par.Key))
                {
                    dgvCuotas.Columns[par.Key]!.HeaderText = par.Value;
                }
            }

            if (dgvCuotas.Columns.Contains("FechaEmision"))
            {
                dgvCuotas.Columns["FechaEmision"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvCuotas.Columns.Contains("FechaVencimiento"))
            {
                dgvCuotas.Columns["FechaVencimiento"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
            }

            if (dgvCuotas.Columns.Contains("Monto"))
            {
                dgvCuotas.Columns["Monto"]!.DefaultCellStyle.Format = "N2";
                dgvCuotas.Columns["Monto"]!.DefaultCellStyle.FormatProvider = CultureInfo.CurrentCulture;
            }

            int orden = 0;
            foreach (var clave in new[] { "IdCuota", "Monto", "FechaEmision", "FechaVencimiento", "Estado", "EnMora" })
            {
                if (dgvCuotas.Columns.Contains(clave))
                {
                    dgvCuotas.Columns[clave]!.DisplayIndex = orden++;
                }
            }
        }

        private void AplicarEstilosFilas()
        {
            foreach (DataGridViewRow row in dgvCuotas.Rows)
            {
                if (row.DataBoundItem is not Cuota cuota)
                {
                    continue;
                }

                if (cuota.Estado == "PAGADA")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                    row.DefaultCellStyle.ForeColor = UiTheme.TextoSecundario;
                }
                else if (cuota.EstaVencida() || cuota.EnMora || cuota.Estado == "VENCIDA")
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = UiTheme.Error;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = UiTheme.Texto;
                }
            }
        }

        private void BtnCobrar_Click(object? sender, EventArgs e)
        {
            if (_socioSeleccionado is null)
            {
                MessageBox.Show("Busque un socio primero.", "Cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dgvCuotas.CurrentRow?.DataBoundItem is not Cuota cuota)
            {
                MessageBox.Show("Seleccione una cuota de la grilla.", "Cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cuota.Estado == "PAGADA")
            {
                MessageBox.Show("La cuota seleccionada ya está pagada.", "Cobro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var monto = numMonto.Value;
            if (monto <= 0)
            {
                MessageBox.Show("Ingrese un monto mayor a cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var recargo = cuota.ObtenerRecargo();
            var totalMinimo = cuota.MontoTotalConRecargo();
            if (cuota.RequiereRecargo() && recargo > 0 && monto < totalMinimo)
            {
                MessageBox.Show(
                    $"E1 — Socio en mora: el cobro debe incluir el recargo adicional.\n\n" +
                    $"Cuota base: {FormatearMonto(cuota.Monto)}\n" +
                    $"Recargo mora: {FormatearMonto(recargo)}\n" +
                    $"Total mínimo: {FormatearMonto(totalMinimo)}",
                    "Recargo obligatorio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                EstablecerMonto(numMonto, totalMinimo);
                return;
            }

            if (chkGenerarProxima.Checked && numMontoProxima.Value <= 0)
            {
                MessageBox.Show("Ingrese un monto válido para la próxima cuota.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var textoConfirmacion = recargo > 0
                ? $"¿Registrar cobro de {FormatearMonto(monto)} por la cuota #{cuota.IdCuota}?\n\n" +
                  $"Incluye recargo por mora: {FormatearMonto(recargo)} (base {FormatearMonto(cuota.Monto)})."
                : $"¿Registrar cobro de {FormatearMonto(monto)} por la cuota #{cuota.IdCuota}?";

            var confirmacion = MessageBox.Show(
                textoConfirmacion,
                "Confirmar cobro",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!_pagoDao.RegistrarPagoSocio(
                        _socioSeleccionado.IdSocio,
                        cuota.IdCuota,
                        monto,
                        cboMedioPago.Text,
                        txtConcepto.Text.Trim(),
                        out var pagoId))
                {
                    MessageBox.Show("No se pudo registrar el pago.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var estabaEnMora = _socioSeleccionado.EstadoCuota == "MORA";
                var sigueEnMora = ActualizarEstadoSocioSegunCuotasPendientesRetornaMora(_socioSeleccionado.IdSocio);

                var mensaje = recargo > 0
                    ? $"Pago #{pagoId} registrado por {FormatearMonto(monto)} (base {FormatearMonto(cuota.Monto)} + recargo {FormatearMonto(recargo)})."
                    : $"Pago #{pagoId} registrado por {FormatearMonto(monto)}.";

                if (estabaEnMora && !sigueEnMora)
                {
                    mensaje += "\nAcceso reactivado: socio actualizado a AL_DIA.";
                }

                if (chkGenerarProxima.Checked)
                {
                    var proxima = new Cuota
                    {
                        SocioId = _socioSeleccionado.IdSocio,
                        Monto = numMontoProxima.Value,
                        FechaVencimiento = DateTime.Today.AddDays(30)
                    };

                    if (_cuotaDao.Crear(proxima, out var nuevaCuotaId))
                    {
                        mensaje += $"\nNueva cuota #{nuevaCuotaId} vence el {proxima.FechaVencimiento:dd/MM/yyyy}.";
                    }
                    else
                    {
                        mensaje += "\nNo se pudo generar la próxima cuota.";
                    }
                }

                MessageBox.Show(mensaje, "Cobro exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BuscarSocio();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// El SP marca AL_DIA al cobrar; si aún hay cuotas vencidas sin pagar, el socio queda en MORA.
        /// </summary>
        /// <returns>true si el socio queda en MORA; false si queda AL_DIA.</returns>
        private bool ActualizarEstadoSocioSegunCuotasPendientesRetornaMora(int socioId)
        {
            var pendientes = _cuotaDao.ObtenerPorSocio(socioId)
                .Where(c => c.Estado != "PAGADA")
                .ToList();

            var enMora = pendientes.Exists(c => c.EstaVencida() || c.EnMora || c.Estado == "VENCIDA");
            var nuevoEstado = enMora ? "MORA" : "AL_DIA";
            _socioDao.ActualizarEstadoCuota(socioId, nuevoEstado);
            return enMora;
        }

        private void LimpiarBusqueda()
        {
            txtDni.Clear();
            _socioSeleccionado = null;
            dgvCuotas.DataSource = null;
            lblSocio.Text = string.Empty;
            lblDetalleCuota.Text = string.Empty;
            btnCobrar.Enabled = false;
            numMonto.Value = 150m;
            numMontoProxima.Value = 150m;
            txtConcepto.Text = "Cuota mensual";
            cboMedioPago.SelectedIndex = 0;
            chkGenerarProxima.Checked = true;
            txtDni.Focus();
        }

        private void SincronizarMontoProxima()
        {
            if (chkGenerarProxima.Checked)
            {
                EstablecerMonto(numMontoProxima, numMonto.Value);
            }
        }

        private static void EstablecerMonto(NumericUpDown control, decimal monto)
        {
            var valor = Math.Clamp(monto, control.Minimum, control.Maximum);
            control.Value = valor;
        }

        private static string FormatearMonto(decimal monto) =>
            monto.ToString("C2", CultureInfo.CurrentCulture);

        private void OcultarColumna(string nombre)
        {
            if (dgvCuotas.Columns.Contains(nombre))
            {
                dgvCuotas.Columns[nombre]!.Visible = false;
            }
        }
    }
}

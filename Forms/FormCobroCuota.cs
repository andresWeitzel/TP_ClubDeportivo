using System.Drawing;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormCobroCuota : Form
    {
        private readonly TextBox txtDni;
        private readonly Label lblSocio;
        private readonly DataGridView dgvCuotas;
        private readonly NumericUpDown numMonto;
        private readonly ComboBox cboMedioPago;
        private readonly TextBox txtConcepto;
        private readonly Button btnBuscar, btnCobrar;
        private readonly CheckBox chkGenerarProxima;
        private readonly NumericUpDown numMontoProxima;

        private readonly SocioDAO _socioDao = new();
        private readonly CuotaDAO _cuotaDao = new();
        private readonly PagoDAO _pagoDao = new();

        private Socio? _socioSeleccionado;

        public FormCobroCuota()
        {
            Text = "Cobro de Cuota Mensual";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(820, 520);
            Font = new Font("Segoe UI", 10F);

            var panelBusqueda = new Panel { Dock = DockStyle.Top, Height = 72, Padding = new Padding(12) };
            panelBusqueda.Controls.Add(new Label { Text = "DNI del socio:", Location = new Point(8, 12), AutoSize = true });
            txtDni = new TextBox { Location = new Point(110, 8), Width = 140 };
            btnBuscar = new Button { Text = "Buscar", Location = new Point(260, 6), Size = new Size(90, 30) };
            btnBuscar.Click += (_, _) => BuscarSocio();
            lblSocio = new Label
            {
                Location = new Point(8, 40),
                Size = new Size(760, 24),
                ForeColor = Color.FromArgb(37, 99, 168),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            panelBusqueda.Controls.AddRange([txtDni, btnBuscar, lblSocio]);

            dgvCuotas = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var panelPago = new GroupBox
            {
                Text = "Registrar pago (CU-03)",
                Dock = DockStyle.Bottom,
                Height = 160,
                Padding = new Padding(12)
            };

            panelPago.Controls.Add(new Label { Text = "Monto:", Location = new Point(16, 32), AutoSize = true });
            numMonto = new NumericUpDown
            {
                Location = new Point(80, 28),
                Width = 100,
                DecimalPlaces = 2,
                Maximum = 999999,
                Minimum = 0.01m,
                Value = 150
            };

            panelPago.Controls.Add(new Label { Text = "Medio:", Location = new Point(200, 32), AutoSize = true });
            cboMedioPago = new ComboBox
            {
                Location = new Point(250, 28),
                Width = 140,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMedioPago.Items.AddRange(["Efectivo", "Tarjeta Débito", "Tarjeta Crédito", "Transferencia"]);
            cboMedioPago.SelectedIndex = 0;

            panelPago.Controls.Add(new Label { Text = "Concepto:", Location = new Point(16, 68), AutoSize = true });
            txtConcepto = new TextBox { Location = new Point(80, 64), Size = new Size(400, 25), Text = "Cuota mensual" };

            chkGenerarProxima = new CheckBox
            {
                Text = "Generar próxima cuota (+30 días)",
                Location = new Point(16, 100),
                AutoSize = true,
                Checked = true
            };

            panelPago.Controls.Add(new Label { Text = "Monto próxima:", Location = new Point(280, 100), AutoSize = true });
            numMontoProxima = new NumericUpDown
            {
                Location = new Point(390, 96),
                Width = 90,
                DecimalPlaces = 2,
                Maximum = 999999,
                Minimum = 0.01m,
                Value = 150
            };

            btnCobrar = new Button
            {
                Text = "Registrar cobro",
                Location = new Point(520, 28),
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(37, 99, 168),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCobrar.FlatAppearance.BorderSize = 0;
            btnCobrar.Click += BtnCobrar_Click;

            panelPago.Controls.AddRange([
                numMonto, cboMedioPago, txtConcepto, chkGenerarProxima, numMontoProxima, btnCobrar
            ]);

            Controls.Add(dgvCuotas);
            Controls.Add(panelPago);
            Controls.Add(panelBusqueda);

            AcceptButton = btnBuscar;
        }

        private void BuscarSocio()
        {
            _socioSeleccionado = null;
            dgvCuotas.DataSource = null;
            lblSocio.Text = string.Empty;

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

                lblSocio.Text = $"{_socioSeleccionado.Nombre} {_socioSeleccionado.Apellido} — Estado cuota: {_socioSeleccionado.EstadoCuota}";
                dgvCuotas.DataSource = _cuotaDao.ObtenerPorSocio(_socioSeleccionado.IdSocio).ToList();

                var pendiente = _cuotaDao.ObtenerUltimaPorSocio(_socioSeleccionado.IdSocio);
                if (pendiente is not null && pendiente.Estado != "PAGADA")
                {
                    numMonto.Value = pendiente.Monto;
                    numMontoProxima.Value = pendiente.Monto;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            try
            {
                var monto = numMonto.Value;
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

                var mensaje = $"Pago #{pagoId} registrado por ${monto:N2}.";

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
                        mensaje += $"\nNueva cuota #{nuevaCuotaId} vence {proxima.FechaVencimiento:dd/MM/yyyy}.";
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
    }
}

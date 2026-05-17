using System.Drawing;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormVisitantes : Form
    {
        private readonly DataGridView dgvVisitantes;
        private readonly TextBox txtDni, txtNombre, txtApellido, txtTelefono, txtActividad, txtMonto;
        private readonly ComboBox cboMedioPago;
        private readonly Button btnRegistrar;
        private readonly Label lblMensaje;

        private readonly VisitanteDAO _visitanteDao = new();
        private readonly PagoDAO _pagoDao = new();

        public FormVisitantes()
        {
            Text = "Ingreso de Visitantes";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(960, 580);
            Font = new Font("Segoe UI", 10F);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 560
            };

            dgvVisitantes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var btnRefrescar = new Button
            {
                Text = "Actualizar",
                Dock = DockStyle.Top,
                Height = 36
            };
            btnRefrescar.Click += (_, _) => CargarVisitantes();

            split.Panel1.Controls.Add(dgvVisitantes);
            split.Panel1.Controls.Add(btnRefrescar);

            var grp = new GroupBox
            {
                Text = "Registrar visitante y pago diario (CU-02)",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            txtDni = CrearCampo(grp, "DNI:", 16);
            txtNombre = CrearCampo(grp, "Nombre:", 52);
            txtApellido = CrearCampo(grp, "Apellido:", 88);
            txtTelefono = CrearCampo(grp, "Teléfono:", 124);
            txtActividad = CrearCampo(grp, "Actividad:", 160);
            txtMonto = CrearCampo(grp, "Pago diario ($):", 196);
            txtMonto.Text = "50";

            grp.Controls.Add(new Label { Text = "Medio de pago:", Location = new Point(16, 236), AutoSize = true });
            cboMedioPago = new ComboBox
            {
                Location = new Point(130, 232),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMedioPago.Items.AddRange(["Efectivo", "Tarjeta Débito", "Tarjeta Crédito", "Transferencia"]);
            cboMedioPago.SelectedIndex = 0;
            grp.Controls.Add(cboMedioPago);

            btnRegistrar = new Button
            {
                Text = "Registrar ingreso",
                Location = new Point(16, 276),
                Size = new Size(160, 36),
                BackColor = Color.FromArgb(37, 99, 168),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRegistrar.FlatAppearance.BorderSize = 0;
            btnRegistrar.Click += BtnRegistrar_Click;

            lblMensaje = new Label
            {
                Location = new Point(16, 324),
                Size = new Size(320, 100),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            grp.Controls.AddRange([btnRegistrar, lblMensaje]);
            split.Panel2.Controls.Add(grp);
            Controls.Add(split);

            Load += (_, _) => CargarVisitantes();
        }

        private static TextBox CrearCampo(Control parent, string etiqueta, int y)
        {
            parent.Controls.Add(new Label { Text = etiqueta, Location = new Point(16, y + 4), AutoSize = true });
            var txt = new TextBox { Location = new Point(130, y), Size = new Size(200, 25) };
            parent.Controls.Add(txt);
            return txt;
        }

        private void CargarVisitantes()
        {
            try
            {
                dgvVisitantes.DataSource = _visitanteDao.ObtenerListado().ToList();
                ConfigurarColumnasGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            if (dgvVisitantes.Columns.Count == 0)
            {
                return;
            }

            var columnas = new Dictionary<string, string>
            {
                ["IdVisitante"] = "Nº",
                ["Dni"] = "DNI",
                ["Nombre"] = "Nombre",
                ["Apellido"] = "Apellido",
                ["Telefono"] = "Teléfono",
                ["Actividad"] = "Actividad",
                ["FechaIngreso"] = "Fecha ingreso",
                ["Monto"] = "Monto ($)",
                ["MedioPago"] = "Medio de pago",
                ["PagoRegistrado"] = "Pago OK"
            };

            foreach (var par in columnas)
            {
                if (dgvVisitantes.Columns.Contains(par.Key))
                {
                    dgvVisitantes.Columns[par.Key]!.HeaderText = par.Value;
                }
            }

            if (dgvVisitantes.Columns.Contains("FechaIngreso"))
            {
                dgvVisitantes.Columns["FechaIngreso"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            }

            if (dgvVisitantes.Columns.Contains("Monto"))
            {
                dgvVisitantes.Columns["Monto"]!.DefaultCellStyle.Format = "N2";
            }
        }

        private void BtnRegistrar_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtActividad.Text))
            {
                MessageBox.Show("Complete al menos nombre y actividad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtMonto.Text.Trim(), out var monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var visitante = new Visitante
                {
                    DNI = txtDni.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Actividad = txtActividad.Text.Trim(),
                    PagoDiarioMonto = monto
                };

                if (!_visitanteDao.Crear(visitante, out var visitanteId))
                {
                    lblMensaje.Text = "No se pudo registrar el visitante.";
                    return;
                }

                var concepto = $"Entrada diaria — {visitante.Actividad}";
                if (!_pagoDao.RegistrarPagoVisitante(visitanteId, monto, cboMedioPago.Text, concepto, out var pagoId))
                {
                    lblMensaje.Text = $"Visitante #{visitanteId} creado. No se registró el pago.";
                }
                else
                {
                    lblMensaje.Text = $"Visitante #{visitanteId} — Pago #{pagoId} por ${monto:N2}.";
                    MessageBox.Show("Ingreso y pago registrados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Limpiar();
                CargarVisitantes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Limpiar()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            txtActividad.Clear();
            txtMonto.Text = "50";
            cboMedioPago.SelectedIndex = 0;
        }
    }
}

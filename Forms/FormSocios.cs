using System.Drawing;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormSocios : Form
    {
        private readonly DataGridView dgvSocios;
        private readonly TextBox txtDni, txtNombre, txtApellido, txtTelefono, txtDireccion, txtEmail, txtMontoCuota;
        private readonly Button btnGuardar, btnLimpiar, btnRefrescar, btnBuscarDni;
        private readonly TextBox txtBuscarDni;
        private readonly Label lblMensaje;

        private readonly SocioDAO _socioDao = new();
        private readonly CarnetDAO _carnetDao = new();
        private readonly CuotaDAO _cuotaDao = new();
        private readonly FichaMedicaDAO _fichaDao = new();

        public FormSocios()
        {
            Text = "Gestión de Socios";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 620);
            MinimumSize = new Size(900, 560);
            Font = new Font("Segoe UI", 10F);

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 620,
                Panel1MinSize = 400,
                Panel2MinSize = 280
            };

            dgvSocios = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };

            var panelGrillaTop = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
            txtBuscarDni = new TextBox { Location = new Point(8, 10), Width = 140, PlaceholderText = "Buscar por DNI" };
            btnBuscarDni = new Button { Text = "Buscar", Location = new Point(156, 8), Size = new Size(80, 28) };
            btnBuscarDni.Click += (_, _) => BuscarPorDni();
            btnRefrescar = new Button { Text = "Actualizar lista", Location = new Point(246, 8), Size = new Size(120, 28) };
            btnRefrescar.Click += (_, _) => CargarSocios();
            panelGrillaTop.Controls.AddRange([txtBuscarDni, btnBuscarDni, btnRefrescar]);

            split.Panel1.Controls.Add(dgvSocios);
            split.Panel1.Controls.Add(panelGrillaTop);

            var grpAlta = new GroupBox
            {
                Text = "Registrar nuevo socio (CU-01)",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            txtDni = CrearCampo(grpAlta, "DNI:", 16);
            txtNombre = CrearCampo(grpAlta, "Nombre:", 52);
            txtApellido = CrearCampo(grpAlta, "Apellido:", 88);
            txtTelefono = CrearCampo(grpAlta, "Teléfono:", 124);
            txtDireccion = CrearCampo(grpAlta, "Dirección:", 160);
            txtEmail = CrearCampo(grpAlta, "Email:", 196);
            txtMontoCuota = CrearCampo(grpAlta, "Monto 1ª cuota:", 232);
            txtMontoCuota.Text = "150";

            btnGuardar = new Button
            {
                Text = "Registrar socio",
                Location = new Point(16, 276),
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(37, 99, 168),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnLimpiar = new Button
            {
                Text = "Limpiar",
                Location = new Point(168, 276),
                Size = new Size(100, 36)
            };
            btnLimpiar.Click += (_, _) => LimpiarFormulario();

            lblMensaje = new Label
            {
                Location = new Point(16, 324),
                Size = new Size(320, 120),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            grpAlta.Controls.AddRange([btnGuardar, btnLimpiar, lblMensaje]);
            split.Panel2.Controls.Add(grpAlta);

            Controls.Add(split);

            Load += (_, _) => CargarSocios();
        }

        private static TextBox CrearCampo(Control parent, string etiqueta, int y)
        {
            parent.Controls.Add(new Label { Text = etiqueta, Location = new Point(16, y + 4), AutoSize = true });
            var txt = new TextBox { Location = new Point(130, y), Size = new Size(200, 25) };
            parent.Controls.Add(txt);
            return txt;
        }

        private void CargarSocios()
        {
            try
            {
                dgvSocios.DataSource = _socioDao.ObtenerTodos().ToList();
                OcultarColumnaSiExiste("Direccion");
                lblMensaje.Text = $"Total: {dgvSocios.Rows.Count} socios.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar socios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarPorDni()
        {
            var dni = txtBuscarDni.Text.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                CargarSocios();
                return;
            }

            try
            {
                var socio = _socioDao.ObtenerPorDni(dni);
                if (socio is null)
                {
                    MessageBox.Show("No se encontró un socio con ese DNI.", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                dgvSocios.DataSource = new List<Socio> { socio };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos())
            {
                return;
            }

            var dni = txtDni.Text.Trim();
            if (_socioDao.ObtenerPorDni(dni) is not null)
            {
                MessageBox.Show("Ya existe un socio con ese DNI.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtMontoCuota.Text.Trim(), out var monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto de cuota válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var socio = new Socio
                {
                    DNI = dni,
                    Nombre = txtNombre.Text.Trim(),
                    Apellido = txtApellido.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                };

                if (!_socioDao.Crear(socio, out var socioId))
                {
                    lblMensaje.Text = "No se pudo registrar el socio.";
                    return;
                }

                var numeroCarnet = $"CARNET-{socioId:D4}";
                var carnet = new Carnet
                {
                    SocioId = socioId,
                    Numero = numeroCarnet,
                    FechaEmision = DateTime.Today,
                    FechaVencimiento = DateTime.Today.AddYears(1)
                };

                if (!_carnetDao.Crear(carnet, out _))
                {
                    lblMensaje.Text = $"Socio #{socioId} creado, pero falló la emisión del carnet.";
                    CargarSocios();
                    return;
                }

                if (!_fichaDao.Crear(socioId, 0, 0, string.Empty, string.Empty, string.Empty, "Sin definir", out _))
                {
                    lblMensaje.Text = $"Socio y carnet OK. No se pudo crear la ficha médica vacía.";
                }

                var cuota = new Cuota
                {
                    SocioId = socioId,
                    Monto = monto,
                    FechaVencimiento = DateTime.Today.AddDays(30)
                };

                if (!_cuotaDao.Crear(cuota, out var cuotaId))
                {
                    lblMensaje.Text = $"Socio #{socioId} y carnet {numeroCarnet}. No se generó la cuota.";
                }
                else
                {
                    lblMensaje.Text = $"Socio #{socioId} registrado.\nCarnet: {numeroCarnet}\nCuota #{cuotaId} vence {cuota.FechaVencimiento:dd/MM/yyyy}.";
                }

                LimpiarFormulario();
                CargarSocios();
                MessageBox.Show("Socio registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtDni.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("Complete DNI, nombre y apellido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void LimpiarFormulario()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            txtDireccion.Clear();
            txtEmail.Clear();
            txtMontoCuota.Text = "150";
        }

        private void OcultarColumnaSiExiste(string nombre)
        {
            if (dgvSocios.Columns.Contains(nombre))
            {
                dgvSocios.Columns[nombre]!.Visible = false;
            }
        }
    }
}

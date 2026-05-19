using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormSocios : Form
    {
        private readonly SplitContainer split;
        private readonly DataGridView dgvSocios;
        private readonly GroupBox grpFormulario;
        private readonly Panel panelModo;
        private readonly Label lblModo;
        private readonly TextBox txtBuscar;
        private readonly TextBox txtDni, txtNombre, txtApellido, txtTelefono, txtDireccion, txtEmail;
        private readonly NumericUpDown numMonto;
        private readonly Label lblMonto;
        private readonly Button btnGuardar, btnNuevo, btnEliminar, btnBuscar, btnRefrescar;
        private readonly Label lblMensaje;

        private readonly SocioDAO _socioDao = new();
        private readonly CarnetDAO _carnetDao = new();
        private readonly CuotaDAO _cuotaDao = new();
        private readonly FichaMedicaDAO _fichaDao = new();

        private int? _socioSeleccionadoId;

        public FormSocios()
        {
            Text = "Gestión de Socios";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1050, 620);
            MinimumSize = new Size(980, 560);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = UiTheme.Fondo
            };

            var panelGrilla = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

            var panelBuscador = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Padding = new Padding(0, 4, 0, 4)
            };

            txtBuscar = new TextBox
            {
                Location = new Point(0, 6),
                Size = new Size(180, 25),
                PlaceholderText = "Buscar por DNI"
            };
            UiTheme.AplicarCampo(txtBuscar);

            btnBuscar = new Button
            {
                Text = "Buscar",
                Location = new Point(188, 4),
                Size = new Size(90, 30)
            };
            UiTheme.AplicarBotonSecundario(btnBuscar);
            btnBuscar.Click += (_, _) => BuscarPorDni();

            btnRefrescar = new Button
            {
                Text = "Actualizar",
                Location = new Point(286, 4),
                Size = new Size(100, 30)
            };
            UiTheme.AplicarBotonSecundario(btnRefrescar);
            btnRefrescar.Click += (_, _) =>
            {
                txtBuscar.Clear();
                CargarSocios();
            };

            panelBuscador.Controls.AddRange([txtBuscar, btnBuscar, btnRefrescar]);

            dgvSocios = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                MultiSelect = false
            };
            dgvSocios.SelectionChanged += DgvSocios_SelectionChanged;

            panelGrilla.Controls.Add(dgvSocios);
            panelGrilla.Controls.Add(panelBuscador);
            split.Panel1.Controls.Add(panelGrilla);

            grpFormulario = new GroupBox
            {
                Text = "Datos del socio",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 16, 12, 12),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            panelModo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = UiTheme.PrimarioClaro,
                Padding = new Padding(10, 8, 10, 8)
            };
            lblModo = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.PrimarioOscuro,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Alta: complete los datos para registrar el socio (CU-01).",
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelModo.Controls.Add(lblModo);

            var panelCampos = new Panel
            {
                Dock = DockStyle.Top,
                Height = 290,
                Padding = new Padding(4, 8, 4, 0)
            };

            const int anchoEtiqueta = 115;
            const int anchoCampo = 230;
            int y = 0;

            txtDni = AgregarCampo(panelCampos, "DNI:", anchoEtiqueta, anchoCampo, ref y);
            txtNombre = AgregarCampo(panelCampos, "Nombre:", anchoEtiqueta, anchoCampo, ref y);
            txtApellido = AgregarCampo(panelCampos, "Apellido:", anchoEtiqueta, anchoCampo, ref y);
            txtTelefono = AgregarCampo(panelCampos, "Teléfono:", anchoEtiqueta, anchoCampo, ref y);
            txtDireccion = AgregarCampo(panelCampos, "Dirección:", anchoEtiqueta, anchoCampo, ref y);
            txtEmail = AgregarCampo(panelCampos, "Email:", anchoEtiqueta, anchoCampo, ref y);

            lblMonto = new Label
            {
                Text = "Cuota inicial ($):",
                Location = new Point(4, y + 4),
                Size = new Size(anchoEtiqueta, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal
            };
            numMonto = new NumericUpDown
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo, 25),
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 999999m,
                Value = 150m,
                ThousandsSeparator = true
            };
            panelCampos.Controls.Add(lblMonto);
            panelCampos.Controls.Add(numMonto);

            var panelBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                Padding = new Padding(4, 12, 4, 0)
            };

            btnGuardar = new Button
            {
                Text = "Registrar socio",
                Location = new Point(4, 0),
                Size = new Size(165, 38)
            };
            UiTheme.AplicarBotonPrimario(btnGuardar);
            btnGuardar.Click += BtnGuardar_Click;

            btnNuevo = new Button
            {
                Text = "Nuevo",
                Location = new Point(178, 0),
                Size = new Size(90, 38)
            };
            UiTheme.AplicarBotonSecundario(btnNuevo);
            btnNuevo.Click += (_, _) => ModoNuevo();

            btnEliminar = new Button
            {
                Text = "Eliminar",
                Location = new Point(276, 0),
                Size = new Size(90, 38),
                Enabled = false
            };
            UiTheme.AplicarBotonSecundario(btnEliminar);
            btnEliminar.Click += BtnEliminar_Click;

            panelBotones.Controls.AddRange([btnGuardar, btnNuevo, btnEliminar]);

            lblMensaje = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = UiTheme.TextoSecundario,
                Padding = new Padding(6, 8, 6, 0),
                Font = new Font("Segoe UI", 9F)
            };

            grpFormulario.Controls.Add(lblMensaje);
            grpFormulario.Controls.Add(panelBotones);
            grpFormulario.Controls.Add(panelCampos);
            grpFormulario.Controls.Add(panelModo);

            split.Panel2.Controls.Add(grpFormulario);
            Controls.Add(split);

            Load += (_, _) =>
            {
                UiTheme.ConfigurarSplitVertical(split, 0.58);
                ModoNuevo();
                CargarSocios();
            };
        }

        private static TextBox AgregarCampo(Panel panel, string etiqueta, int anchoEtiqueta, int anchoCampo, ref int y)
        {
            panel.Controls.Add(new Label
            {
                Text = etiqueta,
                Location = new Point(4, y + 4),
                Size = new Size(anchoEtiqueta, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal
            });
            var txt = new TextBox
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo, 25)
            };
            UiTheme.AplicarCampo(txt);
            panel.Controls.Add(txt);
            y += 36;
            return txt;
        }

        private void DgvSocios_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvSocios.CurrentRow?.DataBoundItem is not Socio fila)
            {
                return;
            }

            ModoEdicion(fila);
        }

        private void ModoEdicion(Socio socio)
        {
            _socioSeleccionadoId = socio.IdSocio;

            grpFormulario.Text = $"Editar socio Nº {socio.IdSocio}";
            lblModo.Text = "Edición: puede modificar los datos personales. El DNI no se puede cambiar.";
            btnGuardar.Text = "Guardar cambios";
            btnEliminar.Enabled = true;

            txtDni.Text = socio.DNI;
            txtDni.ReadOnly = true;
            txtDni.BackColor = UiTheme.PrimarioClaro;
            txtNombre.Text = socio.Nombre;
            txtApellido.Text = socio.Apellido;
            txtTelefono.Text = socio.Telefono;
            txtDireccion.Text = socio.Direccion;
            txtEmail.Text = socio.Email;

            lblMonto.Visible = false;
            numMonto.Visible = false;

            lblMensaje.Text = $"Estado de cuota: {socio.EstadoCuota} — Alta: {socio.FechaAlta:dd/MM/yyyy}.";
        }

        private void ModoNuevo()
        {
            _socioSeleccionadoId = null;
            dgvSocios.ClearSelection();

            grpFormulario.Text = "Nuevo socio (CU-01)";
            lblModo.Text = "Alta: complete los datos para registrar el socio (CU-01).";
            btnGuardar.Text = "Registrar socio";
            btnEliminar.Enabled = false;

            txtDni.ReadOnly = false;
            txtDni.BackColor = Color.White;

            lblMonto.Visible = true;
            numMonto.Visible = true;
            numMonto.Value = 150m;

            LimpiarCampos();
            lblMensaje.Text = string.Empty;
        }

        private void CargarSocios()
        {
            var idPrevio = _socioSeleccionadoId;

            try
            {
                dgvSocios.DataSource = _socioDao.ObtenerTodos().ToList();
                ConfigurarColumnasGrilla();

                if (idPrevio.HasValue)
                {
                    foreach (DataGridViewRow row in dgvSocios.Rows)
                    {
                        if (row.DataBoundItem is Socio s && s.IdSocio == idPrevio.Value)
                        {
                            row.Selected = true;
                            dgvSocios.CurrentCell = row.Cells[0];
                            return;
                        }
                    }
                }

                dgvSocios.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar socios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            if (dgvSocios.Columns.Count == 0)
            {
                return;
            }

            OcultarColumnaSiExiste("Direccion");
            OcultarColumnaSiExiste("NumeroSocio");
            OcultarColumnaSiExiste("FechaNacimiento");

            var columnas = new Dictionary<string, string>
            {
                ["IdSocio"] = "Nº",
                ["DNI"] = "DNI",
                ["Nombre"] = "Nombre",
                ["Apellido"] = "Apellido",
                ["Telefono"] = "Teléfono",
                ["Email"] = "Email",
                ["EstadoCuota"] = "Estado cuota",
                ["FechaAlta"] = "Fecha alta"
            };

            foreach (var par in columnas)
            {
                if (dgvSocios.Columns.Contains(par.Key))
                {
                    dgvSocios.Columns[par.Key]!.HeaderText = par.Value;
                }
            }

            if (dgvSocios.Columns.Contains("FechaAlta"))
            {
                dgvSocios.Columns["FechaAlta"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
                dgvSocios.Columns["FechaAlta"]!.DefaultCellStyle.FormatProvider = CultureInfo.CurrentCulture;
            }

            int orden = 0;
            foreach (var clave in new[] { "IdSocio", "DNI", "Nombre", "Apellido", "Telefono", "Email", "EstadoCuota", "FechaAlta" })
            {
                if (dgvSocios.Columns.Contains(clave))
                {
                    dgvSocios.Columns[clave]!.DisplayIndex = orden++;
                }
            }
        }

        private void BuscarPorDni()
        {
            var dni = txtBuscar.Text.Trim();
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
                ConfigurarColumnasGrilla();
                dgvSocios.Rows[0].Selected = true;
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

            try
            {
                if (_socioSeleccionadoId.HasValue)
                {
                    ActualizarSocio();
                }
                else
                {
                    CrearSocio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearSocio()
        {
            var dni = txtDni.Text.Trim();
            if (_socioDao.ObtenerPorDni(dni) is not null)
            {
                MessageBox.Show("Ya existe un socio con ese DNI.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var monto = numMonto.Value;
            if (monto <= 0)
            {
                MessageBox.Show("Ingrese un monto de cuota válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

            var avisos = new List<string>();

            if (!_carnetDao.Crear(carnet, out _))
            {
                avisos.Add("no se pudo emitir el carnet");
            }

            if (!_fichaDao.Crear(socioId, 0, 0, string.Empty, string.Empty, string.Empty, "Sin definir", out _))
            {
                avisos.Add("no se pudo crear la ficha médica");
            }

            var cuota = new Cuota
            {
                SocioId = socioId,
                Monto = monto,
                FechaVencimiento = DateTime.Today.AddDays(30)
            };

            if (!_cuotaDao.Crear(cuota, out var cuotaId))
            {
                avisos.Add("no se generó la cuota");
                lblMensaje.Text = $"Socio #{socioId} creado, pero {string.Join(", ", avisos)}.";
            }
            else if (avisos.Count > 0)
            {
                lblMensaje.Text = $"Socio #{socioId} y cuota #{cuotaId} creados, pero {string.Join(", ", avisos)}.";
            }
            else
            {
                lblMensaje.Text = $"Socio #{socioId} registrado. Carnet {numeroCarnet}. Cuota #{cuotaId} vence el {cuota.FechaVencimiento:dd/MM/yyyy}.";
            }

            MessageBox.Show("Socio registrado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoNuevo();
            CargarSocios();
        }

        private void ActualizarSocio()
        {
            var socio = new Socio
            {
                IdSocio = _socioSeleccionadoId!.Value,
                DNI = txtDni.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Direccion = txtDireccion.Text.Trim(),
                Email = txtEmail.Text.Trim()
            };

            if (!_socioDao.Actualizar(socio))
            {
                lblMensaje.Text = "No se pudieron guardar los cambios del socio.";
                return;
            }

            lblMensaje.Text = $"Socio #{socio.IdSocio} actualizado correctamente.";
            MessageBox.Show("Cambios guardados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarSocios();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (!_socioSeleccionadoId.HasValue)
            {
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Eliminar al socio #{_socioSeleccionadoId.Value}?\n\n" +
                "Se eliminarán también todos sus registros asociados:\n" +
                "  • Pagos\n" +
                "  • Cuotas\n" +
                "  • Carnets\n" +
                "  • Ficha médica\n" +
                "  • Rutinas\n" +
                "  • Turnos de nutrición\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes)
            {
                return;
            }

            if (!_socioDao.Eliminar(_socioSeleccionadoId.Value, out var mensaje))
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(mensaje) ? "No se pudo eliminar el socio." : mensaje,
                    "No se eliminó",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(mensaje, "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoNuevo();
            CargarSocios();
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

        private void LimpiarCampos()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            txtDireccion.Clear();
            txtEmail.Clear();
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormRutinas : Form
    {
        private readonly SocioDAO _socioDao = new();
        private readonly ProfesorDAO _profesorDao = new();
        private readonly FichaMedicaDAO _fichaMedicaDao = new();
        private readonly RutinaDAO _rutinaDao = new();

        private Socio? _socioSeleccionado;
        private (int Id, int SocioId, string SocioNombre, decimal Peso, decimal Altura, string Alergias, string Medicacion, string Observaciones, string CargaPermitida)? _fichaSeleccionada;
        private List<RutinaViewModel> _rutinas = new();

        private readonly TextBox txtDni;
        private readonly Button btnBuscarSocio;
        private readonly Button btnLimpiarBusqueda;
        private readonly Label lblSocio;
        private readonly Label lblEstadoCuota;
        private readonly Label lblFichaMedica;
        private readonly ComboBox cbProfesores;
        private readonly TextBox txtDescripcion;
        private readonly TextBox txtObservaciones;
        private readonly Button btnCrearRutina;
        private readonly DataGridView dgvRutinas;
        private readonly Label lblMensaje;

        public FormRutinas()
        {
            Text = "Confeccionar rutina (CU-06)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1020, 720);
            MinimumSize = new Size(940, 560);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 230,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(24)
            };

            var lblTitulo = new Label
            {
                Text = "Confeccionar rutina para socio",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UiTheme.Texto,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblDescripcion = new Label
            {
                Text = "Busque un socio por DNI, verifique su estado de cuota y ficha médica, y registre la rutina.",
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario,
                AutoSize = true,
                Location = new Point(0, 38)
            };

            var lblDni = new Label
            {
                Text = "DNI del socio:",
                AutoSize = true,
                Location = new Point(0, 92),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            txtDni = new TextBox
            {
                Width = 220,
                Location = new Point(0, 116)
            };
            UiTheme.AplicarCampo(txtDni);
            txtDni.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BuscarSocio();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };

            btnBuscarSocio = new Button
            {
                Text = "Buscar socio",
                AutoSize = true,
                Location = new Point(244, 112)
            };
            UiTheme.AplicarBotonPrimario(btnBuscarSocio);
            btnBuscarSocio.Click += (_, _) => BuscarSocio();

            btnLimpiarBusqueda = new Button
            {
                Text = "Limpiar",
                AutoSize = true,
                Location = new Point(356, 112)
            };
            UiTheme.AplicarBotonSecundario(btnLimpiarBusqueda);
            btnLimpiarBusqueda.Click += (_, _) => LimpiarBusqueda();

            lblSocio = new Label
            {
                Text = "Socio: -",
                AutoSize = true,
                Location = new Point(0, 150),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            lblEstadoCuota = new Label
            {
                Text = "Estado de cuota: -",
                AutoSize = true,
                Location = new Point(0, 174),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            lblFichaMedica = new Label
            {
                Text = "Ficha médica: -",
                AutoSize = true,
                MaximumSize = new Size(900, 0),
                Location = new Point(0, 198),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            panelSuperior.Controls.AddRange([lblTitulo, lblDescripcion, lblDni, txtDni, btnBuscarSocio, btnLimpiarBusqueda, lblSocio, lblEstadoCuota, lblFichaMedica]);

            var panelRutinas = new Panel
            {
                Dock = DockStyle.Top,
                Height = 260,
                Padding = new Padding(0),
                BackColor = UiTheme.Fondo
            };

            var lblRutinas = new Label
            {
                Text = "Rutinas del socio",
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = UiTheme.Texto
            };

            dgvRutinas = CrearGrilla();
            dgvRutinas.Dock = DockStyle.Fill;

            panelRutinas.Controls.Add(dgvRutinas);
            panelRutinas.Controls.Add(lblRutinas);

            var panelFormulario = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 16, 0),
                AutoScroll = true
            };

            var lblProfesor = new Label
            {
                Text = "Profesores disponibles:",
                AutoSize = true,
                Location = new Point(0, 0),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            cbProfesores = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 520,
                Location = new Point(0, 28)
            };
            cbProfesores.SelectedIndexChanged += (_, _) => ActualizarEstadoFormulario();

            var lblDescripcionRutina = new Label
            {
                Text = "Descripción de la rutina:",
                AutoSize = true,
                Location = new Point(0, 72),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            txtDescripcion = new TextBox
            {
                Multiline = true,
                Width = 520,
                Height = 90,
                Location = new Point(0, 98),
                ScrollBars = ScrollBars.Vertical
            };
            UiTheme.AplicarCampo(txtDescripcion);
            txtDescripcion.TextChanged += (_, _) => ActualizarEstadoFormulario();

            var lblObservaciones = new Label
            {
                Text = "Observaciones:",
                AutoSize = true,
                Location = new Point(0, 190),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            txtObservaciones = new TextBox
            {
                Multiline = true,
                Width = 520,
                Height = 80,
                Location = new Point(0, 216),
                ScrollBars = ScrollBars.Vertical
            };
            UiTheme.AplicarCampo(txtObservaciones);

            panelFormulario.Controls.AddRange([lblProfesor, cbProfesores, lblDescripcionRutina, txtDescripcion, lblObservaciones, txtObservaciones]);

            var panelPie = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = UiTheme.Tarjeta
            };

            btnCrearRutina = new Button
            {
                Text = "Guardar rutina",
                Size = new Size(180, 38),
                Location = new Point(0, 14),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            UiTheme.AplicarBotonPrimario(btnCrearRutina);
            btnCrearRutina.Click += (_, _) => GuardarRutina();

            lblMensaje = new Label
            {
                Text = "Busque un socio para continuar.",
                AutoSize = true,
                Location = new Point(196, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };

            panelPie.Controls.AddRange([btnCrearRutina, lblMensaje]);
            Controls.Add(panelPie);
            Controls.Add(panelFormulario);
            Controls.Add(panelRutinas);
            Controls.Add(panelSuperior);

            Load += (_, _) =>
            {
                if (!Permisos.ValidarAccesoAlAbrir(this, Permisos.Modulo.Rutinas))
                {
                    return;
                }

                CargarProfesores();
            };
        }

        private void CargarProfesores()
        {
            try
            {
                var profesores = _profesorDao.ObtenerTodos().ToList();
                cbProfesores.Items.Clear();

                foreach (var profesor in profesores)
                {
                    cbProfesores.Items.Add(new ProfesorItem(profesor));
                }

                if (cbProfesores.Items.Count > 0)
                {
                    cbProfesores.SelectedIndex = 0;
                }

                ActualizarEstadoFormulario();
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar los profesores. Verifique la conexión a la base de datos.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BuscarSocio()
        {
            var dni = txtDni.Text.Trim();
            if (string.IsNullOrEmpty(dni))
            {
                MessageBox.Show("Ingrese un DNI.", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _socioSeleccionado = _socioDao.ObtenerPorDni(dni);
                _fichaSeleccionada = null;
                _rutinas = new List<RutinaViewModel>();

                if (_socioSeleccionado is null)
                {
                    lblSocio.Text = "Socio: no encontrado.";
                    lblEstadoCuota.Text = "Estado de cuota: -";
                    lblFichaMedica.Text = "Ficha médica: -";
                    dgvRutinas.DataSource = null;
                    lblMensaje.Text = "No se encontró un socio con ese DNI.";
                    btnCrearRutina.Enabled = false;
                    return;
                }

                _fichaSeleccionada = _fichaMedicaDao.ObtenerPorSocio(_socioSeleccionado.IdSocio);
                lblSocio.Text = $"Socio: {_socioSeleccionado.Nombre} {_socioSeleccionado.Apellido} (#{_socioSeleccionado.IdSocio})";
                lblEstadoCuota.Text = $"Estado de cuota: {_socioSeleccionado.EstadoCuota}";
                lblFichaMedica.Text = _fichaSeleccionada is null
                    ? "Ficha médica: no disponible"
                    : $"Ficha médica: Carga permitida {_fichaSeleccionada.Value.CargaPermitida}. Peso {_fichaSeleccionada.Value.Peso}kg, Altura {_fichaSeleccionada.Value.Altura}m.";

                CargarRutinas();
                ActualizarEstadoFormulario();
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al buscar el socio. Verifique la conexión a la base de datos.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarBusqueda()
        {
            txtDni.Clear();
            _socioSeleccionado = null;
            _fichaSeleccionada = null;
            _rutinas.Clear();
            dgvRutinas.DataSource = null;
            lblSocio.Text = "Socio: -";
            lblEstadoCuota.Text = "Estado de cuota: -";
            lblFichaMedica.Text = "Ficha médica: -";
            lblMensaje.Text = "Busque un socio para continuar.";
            txtDescripcion.Clear();
            txtObservaciones.Clear();
            ActualizarEstadoFormulario();
        }

        private void CargarRutinas()
        {
            if (_socioSeleccionado is null)
            {
                dgvRutinas.DataSource = null;
                return;
            }

            try
            {
                var rutinas = _rutinaDao.ObtenerPorSocio(_socioSeleccionado.IdSocio)
                    .Select(r => new RutinaViewModel
                    {
                        Id = r.Id,
                        ProfesorNombre = r.ProfesorNombre,
                        FechaCreacion = r.FechaCreacion,
                        Descripcion = r.Descripcion,
                        Observaciones = r.Observaciones
                    })
                    .ToList();

                _rutinas = rutinas;
                dgvRutinas.DataSource = _rutinas;
                ConfigurarColumnasRutinas();
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar las rutinas del socio. Verifique la conexión a la base de datos.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarRutina()
        {
            if (_socioSeleccionado is null)
            {
                MessageBox.Show("Busque un socio antes de guardar la rutina.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_socioSeleccionado.VerificarEstadoCuota())
            {
                MessageBox.Show("E1: Socio suspendido. No se puede confeccionar una rutina.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_fichaSeleccionada is null)
            {
                MessageBox.Show("El socio no tiene ficha médica. No se puede confeccionar una rutina.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbProfesores.SelectedItem is not ProfesorItem profesorItem)
            {
                MessageBox.Show("Seleccione un profesor para asignar la rutina.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var descripcion = txtDescripcion.Text.Trim();
            if (string.IsNullOrEmpty(descripcion))
            {
                MessageBox.Show("Ingrese la descripción de la rutina.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_rutinaDao.Crear(_socioSeleccionado.IdSocio, profesorItem.Profesor.IdProfesor, descripcion, txtObservaciones.Text.Trim(), out _))
                {
                    MessageBox.Show("Rutina creada correctamente.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtDescripcion.Clear();
                    txtObservaciones.Clear();
                    CargarRutinas();
                    ActualizarEstadoFormulario();
                    return;
                }

                MessageBox.Show("No se pudo guardar la rutina. Intente nuevamente.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al guardar la rutina. Verifique la conexión y vuelva a intentar.", "Confeccionar rutina", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarEstadoFormulario()
        {
            if (_socioSeleccionado is null)
            {
                lblMensaje.Text = "Busque un socio para continuar.";
                btnCrearRutina.Enabled = false;
                return;
            }

            if (!_socioSeleccionado.VerificarEstadoCuota())
            {
                lblMensaje.Text = "E1: Socio suspendido. No puede confeccionar la rutina.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnCrearRutina.Enabled = false;
                return;
            }

            if (_fichaSeleccionada is null)
            {
                lblMensaje.Text = "Socio sin ficha médica. No puede confeccionar la rutina.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnCrearRutina.Enabled = false;
                return;
            }

            if (cbProfesores.SelectedItem is not ProfesorItem)
            {
                lblMensaje.Text = "Seleccione un profesor para asignar la rutina.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnCrearRutina.Enabled = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
            {
                lblMensaje.Text = "Ingrese la descripción de la rutina.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnCrearRutina.Enabled = false;
                return;
            }

            lblMensaje.Text = "Socio listo. Complete la rutina y presione Guardar rutina.";
            lblMensaje.ForeColor = Color.DarkGreen;
            btnCrearRutina.Enabled = true;
        }

        private void ConfigurarColumnasRutinas()
        {
            if (dgvRutinas.Columns.Count == 0)
            {
                return;
            }

            foreach (DataGridViewColumn columna in dgvRutinas.Columns)
            {
                columna.Visible = columna.Name is not "Id";
                columna.ReadOnly = true;
            }

            if (dgvRutinas.Columns.Contains("ProfesorNombre"))
            {
                var columnaProfesor = dgvRutinas.Columns["ProfesorNombre"];
                columnaProfesor.HeaderText = "Profesor";
                columnaProfesor.FillWeight = 18;
            }

            if (dgvRutinas.Columns.Contains("FechaCreacion"))
            {
                var columnaFecha = dgvRutinas.Columns["FechaCreacion"];
                columnaFecha.HeaderText = "Fecha de creación";
                columnaFecha.DefaultCellStyle.Format = "dd/MM/yyyy";
                columnaFecha.FillWeight = 16;
            }

            if (dgvRutinas.Columns.Contains("Descripcion"))
            {
                var columnaDescripcion = dgvRutinas.Columns["Descripcion"];
                columnaDescripcion.HeaderText = "Descripción";
                columnaDescripcion.FillWeight = 30;
            }

            if (dgvRutinas.Columns.Contains("Observaciones"))
            {
                var columnaObservaciones = dgvRutinas.Columns["Observaciones"];
                columnaObservaciones.HeaderText = "Observaciones";
                columnaObservaciones.FillWeight = 24;
            }

            dgvRutinas.AutoResizeColumns();
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

        private sealed class ProfesorItem
        {
            public Profesor Profesor { get; }

            public ProfesorItem(Profesor profesor)
            {
                Profesor = profesor;
            }

            public override string ToString() => $"{Profesor.Nombre} {Profesor.Apellido} ({Profesor.Especialidad})";
        }

        private sealed class RutinaViewModel
        {
            public int Id { get; set; }
            public string ProfesorNombre { get; set; } = string.Empty;
            public DateTime FechaCreacion { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public string Observaciones { get; set; } = string.Empty;
        }
    }
}

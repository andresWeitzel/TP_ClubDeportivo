using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormTurnosNutricion : Form
    {
        private readonly SocioDAO _socioDao = new();
        private readonly NutricionistaDAO _nutricionistaDao = new();
        private readonly TurnoNutricionDAO _turnoDao = new();

        private Socio? _socioSeleccionado;
        private List<TurnoDisponibleViewModel> _turnosDisponibles = new();
        private List<TurnoSocioViewModel> _turnosSocio = new();

        private readonly TextBox txtDni;
        private readonly Button btnBuscarSocio;
        private readonly Label lblSocio;
        private readonly Label lblEstadoCuota;
        private readonly ComboBox cbNutricionistas;
        private readonly DateTimePicker dtpFecha;
        private readonly ComboBox cbHorasDisponibles;
        private readonly Button btnBuscarSiguienteSemana;
        private readonly Button btnAsignarTurno;
        private readonly Label lblFechaDisponibles;
        private readonly Label lblMensaje;
        private readonly DataGridView dgvTurnosSocio;
        private bool _autoAdvanceToAvailableDate;

        public FormTurnosNutricion()
        {
            Text = "Gestionar turno de nutrición (CU-07)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1040, 700);
            MinimumSize = new Size(940, 620);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(16),
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
                Location = new Point(16, 20),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            txtDni = new TextBox
            {
                Location = new Point(110, 16),
                Width = 180,
                PlaceholderText = "Ej: 12345678"
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
                Location = new Point(300, 14),
                Size = new Size(120, 34)
            };
            UiTheme.AplicarBotonPrimario(btnBuscarSocio);
            btnBuscarSocio.Click += (_, _) => BuscarSocio();

            lblSocio = new Label
            {
                Text = "Socio: -",
                Location = new Point(16, 60),
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = UiTheme.Primario
            };

            lblEstadoCuota = new Label
            {
                Text = "Estado cuota: -",
                Location = new Point(16, 86),
                AutoSize = true,
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.TextoSecundario
            };

            panelBusqueda.Controls.AddRange([txtDni, btnBuscarSocio, lblSocio, lblEstadoCuota]);

            dgvTurnosSocio = CrearGrilla();
            dgvTurnosSocio.Dock = DockStyle.Fill;

            var panelGrilla = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = UiTheme.Fondo
            };
            panelGrilla.Controls.Add(dgvTurnosSocio);

            var panelAsignacion = new GroupBox
            {
                Text = "Asignar turno nutrición",
                Dock = DockStyle.Bottom,
                Height = 240,
                Padding = new Padding(16),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = UiTheme.Tarjeta
            };

            panelAsignacion.Controls.Add(new Label
            {
                Text = "Nutricionista:",
                Location = new Point(16, 34),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            cbNutricionistas = new ComboBox
            {
                Location = new Point(130, 30),
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbNutricionistas.SelectedIndexChanged += (_, _) =>
            {
                CargarTurnosDisponibles();
                ActualizarEstadoFormulario();
            };

            panelAsignacion.Controls.Add(new Label
            {
                Text = "Fecha:",
                Location = new Point(16, 78),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            dtpFecha = new DateTimePicker
            {
                Location = new Point(130, 74),
                Width = 180,
                Format = DateTimePickerFormat.Short,
                MinDate = DateTime.Today
            };
            dtpFecha.ValueChanged += (_, _) =>
            {
                CargarTurnosDisponibles();
                ActualizarEstadoFormulario();
            };

            panelAsignacion.Controls.Add(new Label
            {
                Text = "Hora disponible:",
                Location = new Point(16, 122),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            cbHorasDisponibles = new ComboBox
            {
                Location = new Point(130, 118),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbHorasDisponibles.SelectedIndexChanged += (_, _) => ActualizarEstadoFormulario();

            lblFechaDisponibles = new Label
            {
                Text = "Turnos disponibles para: -",
                Location = new Point(130, 146),
                AutoSize = true,
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.TextoSecundario
            };

            btnBuscarSiguienteSemana = new Button
            {
                Text = "Buscar próxima fecha",
                Location = new Point(330, 110),
                Size = new Size(180, 34)
            };
            UiTheme.AplicarBotonSecundario(btnBuscarSiguienteSemana);
            btnBuscarSiguienteSemana.Click += (_, _) =>
            {
                dtpFecha.Value = dtpFecha.Value.AddDays(7);
                CargarTurnosDisponibles();
                ActualizarEstadoFormulario();
            };

            btnAsignarTurno = new Button
            {
                Text = "Asignar turno",
                Size = new Size(180, 40),
                Location = new Point(16, 168),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };
            UiTheme.AplicarBotonPrimario(btnAsignarTurno);
            btnAsignarTurno.Click += (_, _) => AsignarTurno();

            lblMensaje = new Label
            {
                Text = "Busque un socio y seleccione un turno disponible.",
                Location = new Point(220, 176),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiTheme.TextoSecundario
            };

            panelAsignacion.Controls.AddRange([cbNutricionistas, dtpFecha, cbHorasDisponibles, lblFechaDisponibles, btnBuscarSiguienteSemana, btnAsignarTurno, lblMensaje]);

            var panelSuperior = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            panelSuperior.Controls.Add(panelGrilla);
            panelSuperior.Controls.Add(panelAsignacion);

            Controls.Add(panelSuperior);
            Controls.Add(panelBusqueda);

            Load += (_, _) =>
            {
                CargarNutricionistas();
                ActualizarEstadoFormulario();
            };
        }

        private void CargarNutricionistas()
        {
            try
            {
                var nutricionistas = _nutricionistaDao.ObtenerTodos().ToList();
                cbNutricionistas.Items.Clear();

                foreach (var nutricionista in nutricionistas)
                {
                    cbNutricionistas.Items.Add(new NutricionistaItem(nutricionista));
                }

                if (cbNutricionistas.Items.Count > 0)
                {
                    cbNutricionistas.SelectedIndex = 0;
                }
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar los nutricionistas. Verifique la conexión a la base de datos.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                if (_socioSeleccionado is null)
                {
                    lblSocio.Text = "Socio: no encontrado.";
                    lblEstadoCuota.Text = "Estado cuota: -";
                    dgvTurnosSocio.DataSource = null;
                    lblMensaje.Text = "No se encontró un socio con ese DNI.";
                    lblMensaje.ForeColor = Color.DarkRed;
                    btnAsignarTurno.Enabled = false;
                    return;
                }

                lblSocio.Text = $"Socio: {_socioSeleccionado.Nombre} {_socioSeleccionado.Apellido} (#{_socioSeleccionado.IdSocio})";
                lblEstadoCuota.Text = $"Estado cuota: {_socioSeleccionado.EstadoCuota}";
                lblEstadoCuota.ForeColor = _socioSeleccionado.EstadoCuota.Equals("AL_DIA", StringComparison.OrdinalIgnoreCase)
                    ? Color.DarkGreen
                    : Color.OrangeRed;

                CargarTurnosSocio();
                CargarTurnosDisponibles();
                ActualizarEstadoFormulario();
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al buscar el socio. Verifique la conexión a la base de datos.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarTurnosSocio()
        {
            if (_socioSeleccionado is null)
            {
                dgvTurnosSocio.DataSource = null;
                return;
            }

            try
            {
                _turnosSocio = _turnoDao.ObtenerPorSocio(_socioSeleccionado.IdSocio)
                    .Select(t => new TurnoSocioViewModel
                    {
                        Id = t.Id,
                        Nutricionista = t.NutricionistaNombre,
                        Fecha = t.Fecha,
                        Hora = t.Hora,
                        Estado = t.Estado
                    })
                    .ToList();

                dgvTurnosSocio.DataSource = _turnosSocio;
                ConfigurarColumnasTurnosSocio();
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar los turnos del socio. Verifique la conexión.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarTurnosDisponibles()
        {
            if (cbNutricionistas.SelectedItem is not NutricionistaItem nutricionistaItem)
            {
                cbHorasDisponibles.Items.Clear();
                cbHorasDisponibles.Text = string.Empty;
                lblFechaDisponibles.Text = "Turnos disponibles para: -";
                return;
            }

            try
            {
                var fechaSeleccionada = dtpFecha.Value.Date;
                _turnosDisponibles = _turnoDao.ObtenerDisponibles(fechaSeleccionada, nutricionistaItem.Nutricionista.IdNutricionista)
                    .Select(t => new TurnoDisponibleViewModel
                    {
                        Id = t.Id,
                        Fecha = t.Fecha,
                        Hora = t.Hora
                    })
                    .ToList();

                cbHorasDisponibles.Items.Clear();
                lblFechaDisponibles.Text = $"Turnos disponibles para: {fechaSeleccionada:dd/MM/yyyy}";

                if (_turnosDisponibles.Count == 0)
                {
                    var siguienteFecha = BuscarProximaFechaConTurnosDisponibles(fechaSeleccionada, nutricionistaItem.Nutricionista.IdNutricionista);
                    if (siguienteFecha is not null)
                    {
                        _autoAdvanceToAvailableDate = true;
                        dtpFecha.Value = siguienteFecha.Value;
                        return;
                    }

                    cbHorasDisponibles.Text = string.Empty;
                    lblMensaje.Text = "E1: Sin turnos disponibles para esa fecha. Busque la próxima fecha.";
                    lblMensaje.ForeColor = Color.DarkRed;
                    btnAsignarTurno.Enabled = false;
                    return;
                }

                foreach (var turno in _turnosDisponibles)
                {
                    cbHorasDisponibles.Items.Add(new TurnoDisponibleItem(turno));
                }

                cbHorasDisponibles.SelectedIndex = 0;

                if (_autoAdvanceToAvailableDate)
                {
                    lblMensaje.Text = $"Fecha ajustada automáticamente a {fechaSeleccionada:dd/MM/yyyy}. Seleccione la hora disponible.";
                    lblMensaje.ForeColor = UiTheme.Primario;
                    _autoAdvanceToAvailableDate = false;
                }
                else
                {
                    lblMensaje.Text = "Seleccione la hora disponible y asigne el turno.";
                    lblMensaje.ForeColor = UiTheme.TextoSecundario;
                }
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar los turnos disponibles. Verifique la conexión.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DateTime? BuscarProximaFechaConTurnosDisponibles(DateTime fechaInicial, int nutricionistaId)
        {
            for (var dias = 1; dias <= 14; dias++)
            {
                var fecha = fechaInicial.AddDays(dias);
                var turnos = _turnoDao.ObtenerDisponibles(fecha, nutricionistaId);
                if (turnos.Any())
                {
                    return fecha;
                }
            }

            return null;
        }

        private void AsignarTurno()
        {
            if (_socioSeleccionado is null)
            {
                MessageBox.Show("Busque un socio antes de asignar el turno.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbNutricionistas.SelectedItem is not NutricionistaItem nutricionistaItem)
            {
                MessageBox.Show("Seleccione un nutricionista.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cbHorasDisponibles.SelectedItem is not TurnoDisponibleItem turnoItem)
            {
                MessageBox.Show("Seleccione una hora disponible.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (_turnoDao.Crear(_socioSeleccionado.IdSocio, nutricionistaItem.Nutricionista.IdNutricionista, dtpFecha.Value.Date, turnoItem.Turno.Hora, "CONFIRMADO", out _))
                {
                    MessageBox.Show("Turno de nutrición asignado correctamente.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarTurnosSocio();
                    CargarTurnosDisponibles();
                    ActualizarEstadoFormulario();
                    return;
                }

                MessageBox.Show("No se pudo asignar el turno. Intente nuevamente.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al asignar el turno. Verifique la conexión.", "Turnos nutrición", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarEstadoFormulario()
        {
            if (_socioSeleccionado is null)
            {
                lblMensaje.Text = "Busque un socio para continuar.";
                lblMensaje.ForeColor = UiTheme.TextoSecundario;
                btnAsignarTurno.Enabled = false;
                return;
            }

            if (cbNutricionistas.SelectedItem is not NutricionistaItem)
            {
                lblMensaje.Text = "Seleccione un nutricionista.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnAsignarTurno.Enabled = false;
                return;
            }

            if (cbHorasDisponibles.SelectedItem is not TurnoDisponibleItem)
            {
                lblMensaje.Text = "Seleccione una hora disponible.";
                lblMensaje.ForeColor = Color.DarkRed;
                btnAsignarTurno.Enabled = false;
                return;
            }

            lblMensaje.Text = "Turno listo para asignar.";
            lblMensaje.ForeColor = Color.DarkGreen;
            btnAsignarTurno.Enabled = true;
        }

        private void ConfigurarColumnasTurnosSocio()
        {
            if (dgvTurnosSocio.Columns.Count == 0)
            {
                return;
            }

            foreach (DataGridViewColumn columna in dgvTurnosSocio.Columns)
            {
                columna.Visible = columna.Name is not "Id";
                columna.ReadOnly = true;
            }

            if (dgvTurnosSocio.Columns.Contains("Nutricionista"))
            {
                dgvTurnosSocio.Columns["Nutricionista"].HeaderText = "Nutricionista";
                dgvTurnosSocio.Columns["Nutricionista"].FillWeight = 24;
            }

            if (dgvTurnosSocio.Columns.Contains("Fecha"))
            {
                var columnaFecha = dgvTurnosSocio.Columns["Fecha"];
                columnaFecha.HeaderText = "Fecha";
                columnaFecha.DefaultCellStyle.Format = "dd/MM/yyyy";
                columnaFecha.FillWeight = 18;
            }

            if (dgvTurnosSocio.Columns.Contains("Hora"))
            {
                var columnaHora = dgvTurnosSocio.Columns["Hora"];
                columnaHora.HeaderText = "Hora";
                columnaHora.FillWeight = 14;
            }

            if (dgvTurnosSocio.Columns.Contains("Estado"))
            {
                var columnaEstado = dgvTurnosSocio.Columns["Estado"];
                columnaEstado.HeaderText = "Estado";
                columnaEstado.FillWeight = 14;
            }

            dgvTurnosSocio.AutoResizeColumns();
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

        private sealed class NutricionistaItem
        {
            public (int IdNutricionista, string DNI, string Nombre, string Apellido, string Telefono, string Email, string Matricula) Nutricionista { get; }

            public NutricionistaItem((int Id, string DNI, string Nombre, string Apellido, string Telefono, string Email, string Matricula) nutricionista)
            {
                Nutricionista = nutricionista;
            }

            public override string ToString() => $"{Nutricionista.Nombre} {Nutricionista.Apellido} ({Nutricionista.Matricula})";
        }

        private sealed class TurnoDisponibleItem
        {
            public TurnoDisponibleViewModel Turno { get; }

            public TurnoDisponibleItem(TurnoDisponibleViewModel turno)
            {
                Turno = turno;
            }

            public override string ToString() => Turno.Hora.ToString(@"hh\:mm");
        }

        private sealed class TurnoDisponibleViewModel
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public TimeSpan Hora { get; set; }
        }

        private sealed class TurnoSocioViewModel
        {
            public int Id { get; set; }
            public string Nutricionista { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public TimeSpan Hora { get; set; }
            public string Estado { get; set; } = string.Empty;
        }
    }
}

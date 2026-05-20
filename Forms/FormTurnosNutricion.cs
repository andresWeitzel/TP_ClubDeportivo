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
        private static readonly string[] CargasPermitidas =
        [
            "Sin restricciones",
            "Actividad moderada",
            "Entrenamiento controlado",
            "Cardio y máquinas guiadas",
            "Yoga y pilates liviano",
            "Evitar >50kg en espalda",
            "Pendiente de evaluación",
            "Sin definir"
        ];

        private readonly SocioDAO _socioDao = new();
        private readonly NutricionistaDAO _nutricionistaDao = new();
        private readonly TurnoNutricionDAO _turnoDao = new();
        private readonly FichaMedicaDAO _fichaDao = new();

        private Socio? _socioSeleccionado;
        private (int Id, int SocioId, string SocioNombre, decimal Peso, decimal Altura, string Alergias, string Medicacion, string Observaciones, string CargaPermitida)? _fichaSeleccionada;
        private TurnoSocioViewModel? _turnoConsultaSeleccionado;
        private List<TurnoDisponibleViewModel> _turnosDisponibles = new();
        private List<TurnoSocioViewModel> _turnosSocio = new();

        private readonly TextBox txtDni;
        private readonly Button btnBuscarSocio;
        private readonly Label lblSocio;
        private readonly Label lblEstadoCuota;
        private readonly Label lblFichaMedica;
        private readonly ComboBox cbNutricionistas;
        private readonly DateTimePicker dtpFecha;
        private readonly ComboBox cbHorasDisponibles;
        private readonly Button btnBuscarSiguienteSemana;
        private readonly Button btnAsignarTurno;
        private readonly Label lblFechaDisponibles;
        private readonly Label lblMensaje;
        private readonly DataGridView dgvTurnosSocio;
        private readonly NumericUpDown numPeso;
        private readonly NumericUpDown numAltura;
        private readonly TextBox txtAlergias;
        private readonly TextBox txtMedicacion;
        private readonly TextBox txtObservacionesFicha;
        private readonly ComboBox cbCargaPermitida;
        private readonly Button btnGuardarConsulta;
        private readonly Label lblConsultaMensaje;
        private bool _autoAdvanceToAvailableDate;

        public FormTurnosNutricion()
        {
            Text = "Gestionar turno de nutrición (CU-07)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1280, 780);
            MinimumSize = new Size(1100, 700);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelBusqueda = new Panel
            {
                Dock = DockStyle.Top,
                Height = 148,
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

            lblFichaMedica = new Label
            {
                Text = "Ficha médica: -",
                Location = new Point(420, 60),
                AutoSize = true,
                MaximumSize = new Size(700, 0),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.TextoSecundario
            };

            panelBusqueda.Controls.AddRange([txtDni, btnBuscarSocio, lblSocio, lblEstadoCuota, lblFichaMedica]);

            dgvTurnosSocio = CrearGrilla();
            dgvTurnosSocio.Dock = DockStyle.Fill;
            dgvTurnosSocio.SelectionChanged += (_, _) => SeleccionarTurnoParaConsulta();

            var panelConsulta = new GroupBox
            {
                Text = "Consulta — actualizar ficha",
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 12, 12, 8),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = UiTheme.Tarjeta
            };

            var panelConsultaCampos = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0, 0, 4, 0)
            };

            var panelConsultaAcciones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 92,
                Padding = new Padding(0, 8, 0, 0)
            };

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Peso (kg):",
                Location = new Point(12, 28),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            numPeso = new NumericUpDown
            {
                Location = new Point(12, 48),
                Width = 100,
                DecimalPlaces = 2,
                Maximum = 300,
                Minimum = 0,
                Increment = 0.5M
            };

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Altura (m):",
                Location = new Point(130, 28),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            numAltura = new NumericUpDown
            {
                Location = new Point(130, 48),
                Width = 100,
                DecimalPlaces = 2,
                Maximum = 2.5M,
                Minimum = 0,
                Increment = 0.01M
            };

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Alergias:",
                Location = new Point(12, 82),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            txtAlergias = new TextBox
            {
                Location = new Point(12, 102),
                Width = 218,
                PlaceholderText = "Ej: Penicilina"
            };
            UiTheme.AplicarCampo(txtAlergias);

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Medicación:",
                Location = new Point(12, 132),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            txtMedicacion = new TextBox
            {
                Location = new Point(12, 152),
                Width = 218,
                PlaceholderText = "Ej: Ninguna"
            };
            UiTheme.AplicarCampo(txtMedicacion);

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Observaciones:",
                Location = new Point(12, 182),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            txtObservacionesFicha = new TextBox
            {
                Location = new Point(12, 202),
                Width = 218,
                Height = 88,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Notas de la consulta"
            };
            UiTheme.AplicarCampo(txtObservacionesFicha);

            panelConsultaCampos.Controls.Add(new Label
            {
                Text = "Carga actividad permitida:",
                Location = new Point(12, 278),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });

            cbCargaPermitida = new ComboBox
            {
                Location = new Point(12, 286),
                Width = 218,
                DropDownStyle = ComboBoxStyle.DropDown
            };
            cbCargaPermitida.Items.AddRange(CargasPermitidas);

            btnGuardarConsulta = new Button
            {
                Text = "Guardar ficha",
                Dock = DockStyle.Top,
                Height = 38
            };
            UiTheme.AplicarBotonPrimario(btnGuardarConsulta);
            btnGuardarConsulta.Click += (_, _) => GuardarConsulta();
            btnGuardarConsulta.Enabled = false;

            lblConsultaMensaje = new Label
            {
                Text = "Busque un socio para actualizar la ficha médica.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ForeColor = UiTheme.TextoSecundario
            };

            panelConsultaCampos.Controls.AddRange(
            [
                numPeso, numAltura, txtAlergias, txtMedicacion, txtObservacionesFicha, cbCargaPermitida
            ]);

            panelConsultaAcciones.Controls.Add(lblConsultaMensaje);
            panelConsultaAcciones.Controls.Add(btnGuardarConsulta);
            panelConsulta.Controls.Add(panelConsultaCampos);
            panelConsulta.Controls.Add(panelConsultaAcciones);

            void AjustarAnchoCamposConsulta()
            {
                const int margen = 12;
                const int separacion = 10;
                var anchoTotal = Math.Max(280, panelConsultaCampos.ClientSize.Width - margen * 2);
                var anchoMitad = (anchoTotal - separacion) / 2;

                numPeso.Width = anchoMitad;
                numPeso.Location = new Point(margen, 48);

                numAltura.Width = anchoMitad;
                numAltura.Location = new Point(margen + anchoMitad + separacion, 48);

                txtAlergias.Width = anchoTotal;
                txtAlergias.Location = new Point(margen, 102);

                txtMedicacion.Width = anchoTotal;
                txtMedicacion.Location = new Point(margen, 152);

                txtObservacionesFicha.Width = anchoTotal;
                txtObservacionesFicha.Location = new Point(margen, 202);

                cbCargaPermitida.Width = anchoTotal;
                cbCargaPermitida.Location = new Point(margen, 298);
            }

            panelConsultaCampos.Resize += (_, _) => AjustarAnchoCamposConsulta();

            var splitCentral = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BackColor = UiTheme.Fondo
            };
            splitCentral.Panel1.Controls.Add(dgvTurnosSocio);
            splitCentral.Panel2.Controls.Add(panelConsulta);

            Load += (_, _) =>
            {
                UiTheme.ConfigurarSplitVertical(splitCentral, ratioPanel1: 0.38, panel1Min: 240, panel2Min: 320);
                AjustarAnchoCamposConsulta();
            };

            var panelGrilla = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = UiTheme.Fondo
            };
            panelGrilla.Controls.Add(splitCentral);

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
                    lblFichaMedica.Text = "Ficha médica: -";
                    dgvTurnosSocio.DataSource = null;
                    LimpiarConsulta();
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

                CargarFichaMedica();
                CargarTurnosSocio();
                CargarTurnosDisponibles();
                ActualizarEstadoFormulario();
                ActualizarEstadoConsulta();
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
                    MessageBox.Show(
                        "Turno asignado. Selecciónelo en la grilla para registrar la consulta y actualizar la ficha médica.",
                        "Turnos nutrición",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
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

        private void CargarFichaMedica()
        {
            if (_socioSeleccionado is null)
            {
                _fichaSeleccionada = null;
                lblFichaMedica.Text = "Ficha médica: -";
                return;
            }

            try
            {
                _fichaSeleccionada = _fichaDao.ObtenerPorSocio(_socioSeleccionado.IdSocio);
                lblFichaMedica.Text = _fichaSeleccionada is null
                    ? "Ficha médica: no registrada (se creará al guardar la consulta)"
                    : $"Ficha médica: Carga actual \"{_fichaSeleccionada.Value.CargaPermitida}\" — Peso {_fichaSeleccionada.Value.Peso} kg, Altura {_fichaSeleccionada.Value.Altura} m";
                PoblarFormularioConsulta();
            }
            catch
            {
                lblFichaMedica.Text = "Ficha médica: error al cargar";
            }
        }

        private void PoblarFormularioConsulta()
        {
            if (_fichaSeleccionada is null)
            {
                numPeso.Value = 0;
                numAltura.Value = 0;
                txtAlergias.Clear();
                txtMedicacion.Clear();
                txtObservacionesFicha.Clear();
                cbCargaPermitida.Text = string.Empty;
                return;
            }

            var ficha = _fichaSeleccionada.Value;
            numPeso.Value = Math.Min(numPeso.Maximum, Math.Max(numPeso.Minimum, ficha.Peso));
            numAltura.Value = Math.Min(numAltura.Maximum, Math.Max(numAltura.Minimum, ficha.Altura));
            txtAlergias.Text = ficha.Alergias;
            txtMedicacion.Text = ficha.Medicacion;
            txtObservacionesFicha.Text = ficha.Observaciones;
            cbCargaPermitida.Text = ficha.CargaPermitida;
        }

        private void LimpiarConsulta()
        {
            _fichaSeleccionada = null;
            _turnoConsultaSeleccionado = null;
            PoblarFormularioConsulta();
            ActualizarEstadoConsulta();
        }

        private void SeleccionarTurnoParaConsulta()
        {
            _turnoConsultaSeleccionado = dgvTurnosSocio.CurrentRow?.DataBoundItem as TurnoSocioViewModel;
            ActualizarEstadoConsulta();
        }

        private void ActualizarEstadoConsulta()
        {
            if (_socioSeleccionado is null)
            {
                btnGuardarConsulta.Enabled = false;
                lblConsultaMensaje.Text = "Busque un socio para actualizar la ficha médica.";
                lblConsultaMensaje.ForeColor = UiTheme.TextoSecundario;
                return;
            }

            if (_turnoConsultaSeleccionado is { } turno
                && turno.Estado.Equals("CANCELADO", StringComparison.OrdinalIgnoreCase))
            {
                btnGuardarConsulta.Enabled = false;
                lblConsultaMensaje.Text = "No se puede registrar consulta en un turno cancelado.";
                lblConsultaMensaje.ForeColor = UiTheme.Error;
                return;
            }

            btnGuardarConsulta.Enabled = true;

            if (_turnoConsultaSeleccionado is { } turnoSeleccionado)
            {
                lblConsultaMensaje.Text = turnoSeleccionado.Estado.Equals("ATENDIDO", StringComparison.OrdinalIgnoreCase)
                    ? $"Turno {turnoSeleccionado.Fecha:dd/MM/yyyy} ya atendido. Puede actualizar la ficha."
                    : $"Turno {turnoSeleccionado.Fecha:dd/MM/yyyy} {turnoSeleccionado.Hora:hh\\:mm} — actualice la ficha y guarde.";
                lblConsultaMensaje.ForeColor = UiTheme.Primario;
                return;
            }

            lblConsultaMensaje.Text = "Actualice la ficha médica del socio y pulse Guardar ficha.";
            lblConsultaMensaje.ForeColor = UiTheme.TextoSecundario;
        }

        private void GuardarConsulta()
        {
            if (_socioSeleccionado is null)
            {
                MessageBox.Show("Busque un socio antes de registrar la consulta.", "Consulta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numPeso.Value <= 0 || numAltura.Value <= 0)
            {
                MessageBox.Show("Ingrese peso y altura válidos.", "Consulta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cargaPermitida = cbCargaPermitida.Text.Trim();
            if (string.IsNullOrEmpty(cargaPermitida))
            {
                MessageBox.Show("Defina la carga de actividad física permitida.", "Consulta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbCargaPermitida.Focus();
                return;
            }

            var peso = numPeso.Value;
            var altura = numAltura.Value;
            var alergias = txtAlergias.Text.Trim();
            var medicacion = txtMedicacion.Text.Trim();
            var observaciones = txtObservacionesFicha.Text.Trim();

            try
            {
                var guardado = false;
                if (_fichaSeleccionada is null)
                {
                    guardado = _fichaDao.Crear(
                        _socioSeleccionado.IdSocio,
                        peso,
                        altura,
                        alergias,
                        medicacion,
                        observaciones,
                        cargaPermitida,
                        out _);
                }
                else
                {
                    guardado = _fichaDao.Actualizar(
                        _fichaSeleccionada.Value.Id,
                        peso,
                        altura,
                        alergias,
                        medicacion,
                        observaciones,
                        cargaPermitida);
                }

                if (!guardado)
                {
                    MessageBox.Show("No se pudo actualizar la ficha médica.", "Consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (_turnoConsultaSeleccionado is { } turno
                    && !turno.Estado.Equals("CANCELADO", StringComparison.OrdinalIgnoreCase)
                    && !turno.Estado.Equals("ATENDIDO", StringComparison.OrdinalIgnoreCase)
                    && _turnoDao.Actualizar(turno.Id, turno.Fecha, turno.Hora, "ATENDIDO"))
                {
                    turno.Estado = "ATENDIDO";
                }

                CargarFichaMedica();
                CargarTurnosSocio();
                ActualizarEstadoConsulta();

                MessageBox.Show(
                    "Ficha médica actualizada correctamente.",
                    "Consulta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al guardar la consulta. Verifique la conexión.", "Consulta", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (!(_socioSeleccionado.EstadoCuota.Equals("AL_DIA", StringComparison.OrdinalIgnoreCase)
                  || _socioSeleccionado.EstadoCuota.Equals("Al día", StringComparison.OrdinalIgnoreCase)))
            {
                lblMensaje.Text = "Precondición: el socio debe estar activo (cuota al día) para asignar turnos.";
                lblMensaje.ForeColor = Color.DarkRed;
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

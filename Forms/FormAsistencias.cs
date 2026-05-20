using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormAsistencias : Form
    {
        private readonly ProfesorDAO _profesorDao = new();
        private readonly AsistenciaDAO _asistenciaDao = new();

        private readonly ComboBox cbProfesores;
        private readonly DateTimePicker dtpFecha;
        private readonly DataGridView dgvAsistencias;
        private readonly TextBox txtFirma;
        private readonly CheckBox chkPresente;
        private readonly Button btnBuscar;
        private readonly Button btnFirmar;
        private readonly Label lblMensaje;

        private List<AsistenciaViewModel> _asistencias = new();

        public FormAsistencias()
        {
            Text = "Firmar asistencia (CU-05)";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 620);
            MinimumSize = new Size(900, 520);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var panelSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(24)
            };

            var lblTitulo = new Label
            {
                Text = "Firmar asistencia de profesor",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UiTheme.Texto,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblDescripcion = new Label
            {
                Text = "Seleccione un profesor y una fecha. Luego registre o actualice la firma de su asistencia.",
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario,
                AutoSize = true,
                Location = new Point(0, 38)
            };

            cbProfesores = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 340,
                Location = new Point(0, 84)
            };

            dtpFecha = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 140,
                Location = new Point(360, 84),
                Value = DateTime.Today,
                MaxDate = DateTime.Today
            };
            dtpFecha.ValueChanged += (_, _) => CargarAsistencias();

            btnBuscar = new Button
            {
                Text = "Buscar asistencia",
                AutoSize = true,
                Location = new Point(524, 80)
            };
            UiTheme.AplicarBotonPrimario(btnBuscar);
            btnBuscar.Click += (_, _) => CargarAsistencias();

            panelSuperior.Controls.AddRange([lblTitulo, lblDescripcion, cbProfesores, dtpFecha, btnBuscar]);

            dgvAsistencias = CrearGrilla();
            dgvAsistencias.SelectionChanged += (_, _) => ActualizarFormularioDesdeSeleccion();
            dgvAsistencias.DataBindingComplete += (_, _) => ConfigurarColumnasAsistencias();

            var panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24)
            };
            panelContenido.Controls.Add(dgvAsistencias);

            var panelAccion = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = UiTheme.Tarjeta,
                Padding = new Padding(24)
            };

            var lblFirma = new Label
            {
                Text = "Firma:",
                AutoSize = true,
                Location = new Point(0, 18),
                Font = UiTheme.FuenteNormal,
                ForeColor = UiTheme.Texto
            };

            txtFirma = new TextBox
            {
                Width = 520,
                Location = new Point(0, 42)
            };
            UiTheme.AplicarCampo(txtFirma);
            txtFirma.TextChanged += (_, _) => ActualizarBotonFirmar();

            chkPresente = new CheckBox
            {
                Text = "Presente",
                AutoSize = true,
                Location = new Point(0, 84),
                Checked = true,
                ForeColor = UiTheme.Texto
            };

            btnFirmar = new Button
            {
                Text = "Registrar / firmar",
                Size = new Size(180, 38),
                Location = new Point(0, 110)
            };
            UiTheme.AplicarBotonPrimario(btnFirmar);
            btnFirmar.Click += (_, _) => GuardarFirma();

            lblMensaje = new Label
            {
                Text = string.Empty,
                AutoSize = true,
                Location = new Point(220, 118),
                ForeColor = UiTheme.TextoSecundario,
                Font = new Font("Segoe UI", 9F)
            };

            panelAccion.Controls.AddRange([lblFirma, txtFirma, chkPresente, btnFirmar, lblMensaje]);

            Controls.Add(panelContenido);
            Controls.Add(panelAccion);
            Controls.Add(panelSuperior);

            Load += (_, _) => CargarProfesores();
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

                CargarAsistencias();
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar los profesores. Verifique la conexión a la base de datos.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarAsistencias()
        {
            if (cbProfesores.SelectedItem is not ProfesorItem profesorItem)
            {
                return;
            }

            try
            {
                var fechaSeleccionada = dtpFecha.Value.Date;
                var asistenciasProfesor = _asistenciaDao.ObtenerPorProfesor(profesorItem.Profesor.IdProfesor);

                _asistencias = asistenciasProfesor
                    .Where(a => a.Fecha.Date == fechaSeleccionada)
                    .Select(a => new AsistenciaViewModel
                    {
                        Id = a.Id,
                        ProfesorId = a.ProfesorId,
                        Fecha = a.Fecha,
                        Presente = a.Presente,
                        Firma = a.Firma
                    })
                    .ToList();

                dgvAsistencias.DataSource = _asistencias;
                var asistenciaHoy = _asistencias.FirstOrDefault();
                var esHoy = fechaSeleccionada == DateTime.Today;

                if (_asistencias.Any() && esHoy)
                {
                    lblMensaje.Text = $"E1: Ya firmó hoy. Asistencia registrada para {profesorItem}.";
                    btnFirmar.Text = "Asistencia registrada";
                    btnFirmar.Enabled = false;
                    txtFirma.Text = asistenciaHoy?.Firma ?? string.Empty;
                    chkPresente.Checked = asistenciaHoy?.Presente ?? true;
                    txtFirma.Enabled = false;
                    chkPresente.Enabled = false;
                }
                else
                {
                    lblMensaje.Text = _asistencias.Any()
                        ? $"{_asistencias.Count} registro(s) encontrado(s) para {profesorItem}."
                        : "No hay registro para esta fecha. Puede completar la firma y presionar Registrar.";
                    btnFirmar.Text = _asistencias.Any() ? "Actualizar firma" : "Registrar asistencia";
                    btnFirmar.Enabled = !string.IsNullOrWhiteSpace(txtFirma.Text);
                    txtFirma.Enabled = true;
                    chkPresente.Enabled = true;

                    if (!_asistencias.Any())
                    {
                        chkPresente.Checked = true;
                        txtFirma.Text = string.Empty;
                    }
                }

                ActualizarBotonFirmar();
            }
            catch
            {
                MessageBox.Show("No se pudieron cargar las asistencias. Verifique la conexión a la base de datos.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GuardarFirma()
        {
            if (cbProfesores.SelectedItem is not ProfesorItem profesorItem)
            {
                MessageBox.Show("Seleccione un profesor antes de firmar.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var firma = txtFirma.Text.Trim();
            if (string.IsNullOrWhiteSpace(firma))
            {
                MessageBox.Show("Ingrese la firma antes de continuar.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var presente = chkPresente.Checked;
            var fecha = dtpFecha.Value.Date;

            try
            {
                var existeHoy = dtpFecha.Value.Date == DateTime.Today && _asistencias.Any();
                if (existeHoy)
                {
                    MessageBox.Show("E1: Ya registró asistencia hoy. No se puede guardar otra firma para el mismo día.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (dgvAsistencias.CurrentRow?.DataBoundItem is AsistenciaViewModel asistencia && asistencia.Id > 0)
                {
                    if (_asistenciaDao.Actualizar(asistencia.Id, presente, firma))
                    {
                        MessageBox.Show("Firma actualizada correctamente.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarAsistencias();
                        return;
                    }
                }
                else
                {
                    if (_asistenciaDao.Registrar(profesorItem.Profesor.IdProfesor, fecha, presente, firma, out _))
                    {
                        MessageBox.Show("Asistencia registrada y firmada correctamente.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarAsistencias();
                        return;
                    }
                }

                MessageBox.Show("No se pudo guardar la firma. Intente nuevamente.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("Ocurrió un error al guardar la firma. Verifique la conexión y vuelva a intentar.", "Firmar asistencia", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarFormularioDesdeSeleccion()
        {
            if (dgvAsistencias.CurrentRow?.DataBoundItem is AsistenciaViewModel asistencia)
            {
                chkPresente.Checked = asistencia.Presente;
                txtFirma.Text = asistencia.Firma;
                btnFirmar.Text = "Actualizar firma";
            }
            else
            {
                btnFirmar.Text = _asistencias.Any() ? "Actualizar firma" : "Registrar asistencia";
            }

            ActualizarBotonFirmar();
        }

        private void ActualizarBotonFirmar()
        {
            if (!txtFirma.Enabled)
            {
                btnFirmar.Enabled = false;
                return;
            }

            btnFirmar.Enabled = !string.IsNullOrWhiteSpace(txtFirma.Text);
        }

        private void ConfigurarColumnasAsistencias()
        {
            if (dgvAsistencias.Columns.Count == 0)
            {
                return;
            }

            foreach (DataGridViewColumn columna in dgvAsistencias.Columns)
            {
                columna.Visible = columna.Name is not ("Id" or "ProfesorId");
                columna.ReadOnly = true;
            }

            var columnaFecha = dgvAsistencias.Columns[nameof(AsistenciaViewModel.Fecha)];
            columnaFecha.HeaderText = "Fecha";
            columnaFecha.DefaultCellStyle.Format = "dd/MM/yyyy";
            columnaFecha.FillWeight = 20;

            var columnaPresente = dgvAsistencias.Columns[nameof(AsistenciaViewModel.Presente)];
            columnaPresente.HeaderText = "Presente";
            columnaPresente.FillWeight = 12;

            var columnaFirma = dgvAsistencias.Columns[nameof(AsistenciaViewModel.Firma)];
            columnaFirma.HeaderText = "Firma";
            columnaFirma.FillWeight = 30;

            dgvAsistencias.AutoResizeColumns();
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

        private sealed class AsistenciaViewModel
        {
            public int Id { get; set; }
            public int ProfesorId { get; set; }
            public DateTime Fecha { get; set; }
            public bool Presente { get; set; }
            public string Firma { get; set; } = string.Empty;
        }
    }
}

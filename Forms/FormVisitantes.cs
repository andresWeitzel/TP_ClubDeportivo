using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormVisitantes : Form
    {
        private readonly SplitContainer split;
        private readonly DataGridView dgvVisitantes;
        private readonly GroupBox grpFormulario;
        private readonly Panel panelModo;
        private readonly Label lblModo;
        private readonly TextBox txtDni, txtNombre, txtApellido, txtTelefono;
        private readonly ComboBox cboActividad;
        private readonly Label lblCupoActividad;
        private readonly NumericUpDown numMonto;
        private readonly ComboBox cboMedioPago;
        private readonly Button btnGuardar;
        private readonly Button btnNuevo;
        private readonly Button btnEliminar;
        private readonly Button btnRegistrarPago;
        private readonly Label lblMensaje;

        private readonly VisitanteDAO _visitanteDao = new();
        private readonly PagoDAO _pagoDao = new();
        private readonly ActividadDAO _actividadDao = new();

        private List<Actividad> _actividades = [];
        private int? _visitanteSeleccionadoId;
        private bool _modoAlta = true;
        private bool _omitirSeleccionGrilla;
        private bool _sincronizandoActividad;
        private bool _tienePagoRegistrado;

        public FormVisitantes()
        {
            Text = "Ingreso de Visitantes";
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

            var btnRefrescar = new Button
            {
                Text = "Refrescar lista",
                Dock = DockStyle.Top,
                Height = 36
            };
            UiTheme.AplicarBotonSecundario(btnRefrescar);
            btnRefrescar.Click += (_, _) => CargarVisitantes();

            dgvVisitantes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false
            };
            dgvVisitantes.SelectionChanged += DgvVisitantes_SelectionChanged;

            panelGrilla.Controls.Add(dgvVisitantes);
            panelGrilla.Controls.Add(btnRefrescar);
            split.Panel1.Controls.Add(panelGrilla);

            grpFormulario = new GroupBox
            {
                Text = "Datos del visitante",
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
                Text = "Alta: complete los datos y registre el ingreso con su pago.",
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelModo.Controls.Add(lblModo);

            var panelCampos = new Panel
            {
                Dock = DockStyle.Top,
                Height = 280,
                Padding = new Padding(4, 8, 4, 0)
            };

            const int anchoEtiqueta = 115;
            const int anchoCampo = 230;
            int y = 0;

            txtDni = AgregarCampo(panelCampos, "DNI:", anchoEtiqueta, anchoCampo, ref y);
            txtNombre = AgregarCampo(panelCampos, "Nombre:", anchoEtiqueta, anchoCampo, ref y);
            txtApellido = AgregarCampo(panelCampos, "Apellido:", anchoEtiqueta, anchoCampo, ref y);
            txtTelefono = AgregarCampo(panelCampos, "Teléfono:", anchoEtiqueta, anchoCampo, ref y);
            panelCampos.Controls.Add(new Label
            {
                Text = "Actividad:",
                Location = new Point(4, y + 4),
                Size = new Size(anchoEtiqueta, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal
            });
            cboActividad = new ComboBox
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FuenteNormal
            };
            cboActividad.SelectedIndexChanged += CboActividad_SelectedIndexChanged;
            panelCampos.Controls.Add(cboActividad);
            y += 36;

            lblCupoActividad = new Label
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo + 120, 22),
                ForeColor = UiTheme.TextoSecundario,
                Font = new Font("Segoe UI", 8.5F)
            };
            panelCampos.Controls.Add(lblCupoActividad);
            y += 28;

            panelCampos.Controls.Add(new Label
            {
                Text = "Pago diario ($):",
                Location = new Point(4, y + 4),
                Size = new Size(anchoEtiqueta, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal
            });
            numMonto = new NumericUpDown
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo, 25),
                DecimalPlaces = 2,
                Minimum = 0.01m,
                Maximum = 999999m,
                Value = 50m,
                ThousandsSeparator = true
            };
            panelCampos.Controls.Add(numMonto);
            y += 36;

            panelCampos.Controls.Add(new Label
            {
                Text = "Medio de pago:",
                Location = new Point(4, y + 4),
                Size = new Size(anchoEtiqueta, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = UiTheme.FuenteNormal
            });
            cboMedioPago = new ComboBox
            {
                Location = new Point(anchoEtiqueta + 8, y),
                Size = new Size(anchoCampo, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UiTheme.FuenteNormal
            };
            cboMedioPago.Items.AddRange(["Efectivo", "Tarjeta Débito", "Tarjeta Crédito", "Transferencia"]);
            cboMedioPago.SelectedIndex = 0;
            panelCampos.Controls.Add(cboMedioPago);

            var panelBotones = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                Padding = new Padding(4, 12, 4, 0)
            };

            btnGuardar = new Button
            {
                Text = "Registrar ingreso",
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

            btnRegistrarPago = new Button
            {
                Text = "Registrar pago pendiente",
                Location = new Point(4, 46),
                Size = new Size(362, 32),
                Enabled = false,
                Visible = false
            };
            UiTheme.AplicarBotonSecundario(btnRegistrarPago);
            btnRegistrarPago.Click += BtnRegistrarPago_Click;

            panelBotones.Controls.AddRange([btnGuardar, btnNuevo, btnEliminar, btnRegistrarPago]);

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
                RefrescarActividades();
                ModoNuevo();
                CargarVisitantes();
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

        private void DgvVisitantes_SelectionChanged(object? sender, EventArgs e)
        {
            if (_omitirSeleccionGrilla)
            {
                return;
            }

            if (dgvVisitantes.CurrentRow?.DataBoundItem is not VisitanteListado fila)
            {
                return;
            }

            ModoEdicion(fila);
        }

        private void ModoEdicion(VisitanteListado fila)
        {
            _modoAlta = false;
            _visitanteSeleccionadoId = fila.IdVisitante;
            _tienePagoRegistrado = fila.PagoRegistrado == "Sí";

            grpFormulario.Text = $"Editar visitante Nº {fila.IdVisitante}";
            lblModo.Text = _tienePagoRegistrado
                ? "Edición: puede modificar datos, monto y medio de pago del último cobro."
                : "Edición: sin pago registrado. Guarde cambios o use «Registrar pago pendiente».";
            btnGuardar.Text = "Guardar cambios";
            btnEliminar.Enabled = true;

            txtDni.Text = fila.Dni;
            txtNombre.Text = fila.Nombre;
            txtApellido.Text = fila.Apellido;
            txtTelefono.Text = fila.Telefono;
            SeleccionarActividad(fila.ActividadId);
            EstablecerMonto(fila.Monto);

            cboMedioPago.Enabled = true;
            SeleccionarMedioPago(fila.MedioPago);

            btnRegistrarPago.Visible = !_tienePagoRegistrado;
            btnRegistrarPago.Enabled = !_tienePagoRegistrado;

            lblMensaje.Text = _tienePagoRegistrado
                ? "Al guardar se actualizan el visitante y su último pago."
                : "Registre el pago cuando el visitante abone la entrada.";
        }

        private void ModoNuevo()
        {
            _modoAlta = true;
            _visitanteSeleccionadoId = null;
            _tienePagoRegistrado = false;

            _omitirSeleccionGrilla = true;
            dgvVisitantes.ClearSelection();
            if (dgvVisitantes.Rows.Count > 0)
            {
                dgvVisitantes.CurrentCell = null;
            }
            _omitirSeleccionGrilla = false;

            grpFormulario.Text = "Nuevo visitante (CU-02)";
            lblModo.Text = "Alta (CU-02): seleccione actividad con cupo, complete datos y registre el pago diario.";
            RefrescarActividades();
            SeleccionarPrimeraActividadConCupo();
            btnGuardar.Text = "Registrar ingreso";
            btnEliminar.Enabled = false;
            btnRegistrarPago.Visible = false;
            btnRegistrarPago.Enabled = false;
            cboMedioPago.Enabled = true;

            LimpiarCampos();
            lblMensaje.Text = string.Empty;
        }

        private void CargarVisitantes()
        {
            var idPrevio = _modoAlta ? null : _visitanteSeleccionadoId;

            try
            {
                _omitirSeleccionGrilla = true;
                dgvVisitantes.DataSource = _visitanteDao.ObtenerListado().ToList();
                ConfigurarColumnasGrilla();

                if (idPrevio.HasValue)
                {
                    foreach (DataGridViewRow row in dgvVisitantes.Rows)
                    {
                        if (row.DataBoundItem is VisitanteListado v && v.IdVisitante == idPrevio.Value)
                        {
                            row.Selected = true;
                            dgvVisitantes.CurrentCell = row.Cells[0];
                            _omitirSeleccionGrilla = false;
                            return;
                        }
                    }
                }

                dgvVisitantes.ClearSelection();
                if (dgvVisitantes.Rows.Count > 0)
                {
                    dgvVisitantes.CurrentCell = null;
                }
                _omitirSeleccionGrilla = false;
            }
            catch (Exception ex)
            {
                _omitirSeleccionGrilla = false;
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigurarColumnasGrilla()
        {
            if (dgvVisitantes.Columns.Count == 0)
            {
                return;
            }

            if (dgvVisitantes.Columns.Contains("ActividadId"))
            {
                dgvVisitantes.Columns["ActividadId"]!.Visible = false;
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
                dgvVisitantes.Columns["Monto"]!.DefaultCellStyle.FormatProvider = CultureInfo.CurrentCulture;
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos(out var monto))
            {
                return;
            }

            try
            {
                if (_modoAlta)
                {
                    CrearVisitante(monto);
                }
                else
                {
                    ActualizarVisitante(monto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearVisitante(decimal monto)
        {
            var actividadId = ObtenerActividadIdSeleccionada();
            RefrescarActividades(actividadId);

            if (!ObtenerActividadSeleccionada(out var actividad))
            {
                return;
            }

            if (!ValidarCupoDisponible(actividad.IdActividad, null, out var nombre, out var ocupados, out var cupoMaximo))
            {
                return;
            }

            var visitante = ConstruirVisitante(monto, actividad.IdActividad);

            if (!_visitanteDao.Crear(visitante, out var visitanteId))
            {
                InformarSinCupo(nombre, ocupados, cupoMaximo);
                return;
            }

            var concepto = $"Entrada diaria — {actividad.Nombre}";
            if (!_pagoDao.RegistrarPagoVisitante(visitanteId, monto, cboMedioPago.Text, concepto, out var pagoId))
            {
                lblMensaje.Text = $"Visitante #{visitanteId} creado. No se registró el pago.";
            }
            else
            {
                lblMensaje.Text = $"Visitante #{visitanteId} — Pago #{pagoId} por {FormatearMonto(monto)}.";
                MessageBox.Show("Ingreso y pago registrados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            ModoNuevo();
            CargarVisitantes();
        }

        private void ActualizarVisitante(decimal monto)
        {
            var actividadId = ObtenerActividadIdSeleccionada();
            RefrescarActividades(actividadId);

            if (!ObtenerActividadSeleccionada(out var actividad))
            {
                return;
            }

            if (!ValidarCupoDisponible(actividad.IdActividad, _visitanteSeleccionadoId, out var nombre, out var ocupados, out var cupoMaximo))
            {
                return;
            }

            var visitante = ConstruirVisitante(monto, actividad.IdActividad);
            visitante.IdVisitante = _visitanteSeleccionadoId!.Value;

            if (!_visitanteDao.Actualizar(visitante))
            {
                InformarSinCupo(nombre, ocupados, cupoMaximo);
                return;
            }

            if (_tienePagoRegistrado)
            {
                if (!_pagoDao.ActualizarUltimoPagoVisitante(visitante.IdVisitante, monto, cboMedioPago.Text))
                {
                    lblMensaje.Text = "Visitante actualizado, pero no se pudo actualizar el pago.";
                    CargarVisitantes();
                    return;
                }
            }

            lblMensaje.Text = $"Visitante #{visitante.IdVisitante} actualizado correctamente.";
            MessageBox.Show("Cambios guardados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarVisitantes();
        }

        private void BtnRegistrarPago_Click(object? sender, EventArgs e)
        {
            if (!_visitanteSeleccionadoId.HasValue || _tienePagoRegistrado)
            {
                return;
            }

            if (!ValidarCampos(out var monto))
            {
                return;
            }

            if (!ObtenerActividadSeleccionada(out var actividad))
            {
                return;
            }

            RefrescarActividades(ObtenerActividadIdSeleccionada());

            if (!ValidarCupoDisponible(actividad.IdActividad, _visitanteSeleccionadoId, out _, out _, out _))
            {
                return;
            }

            var visitante = ConstruirVisitante(monto, actividad.IdActividad);
            visitante.IdVisitante = _visitanteSeleccionadoId.Value;

            if (!_visitanteDao.Actualizar(visitante))
            {
                MessageBox.Show("No se pudo actualizar el monto del visitante.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var concepto = $"Entrada diaria — {actividad.Nombre}";
            if (!_pagoDao.RegistrarPagoVisitante(_visitanteSeleccionadoId.Value, monto, cboMedioPago.Text, concepto, out var pagoId))
            {
                MessageBox.Show("No se pudo registrar el pago.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Pago #{pagoId} registrado por {FormatearMonto(monto)}.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarVisitantes();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (!_visitanteSeleccionadoId.HasValue)
            {
                return;
            }

            var textoConfirmacion = _tienePagoRegistrado
                ? $"¿Eliminar al visitante #{_visitanteSeleccionadoId.Value}?\n\nTambién se eliminarán sus pagos asociados."
                : $"¿Eliminar al visitante #{_visitanteSeleccionadoId.Value}?";

            if (MessageBox.Show(textoConfirmacion, "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (!_visitanteDao.Eliminar(_visitanteSeleccionadoId.Value, out var mensaje))
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(mensaje) ? "No se pudo eliminar el visitante." : mensaje,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(mensaje, "Eliminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ModoNuevo();
            CargarVisitantes();
        }

        private Visitante ConstruirVisitante(decimal monto, int actividadId)
        {
            return new Visitante
            {
                DNI = txtDni.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                ActividadId = actividadId,
                PagoDiarioMonto = monto
            };
        }

        private bool ValidarCampos(out decimal monto)
        {
            monto = numMonto.Value;
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || cboActividad.SelectedItem is null)
            {
                MessageBox.Show("Complete al menos nombre y seleccione una actividad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (monto <= 0)
            {
                MessageBox.Show("Ingrese un monto mayor a cero.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void EstablecerMonto(decimal monto)
        {
            var valor = Math.Clamp(monto, numMonto.Minimum, numMonto.Maximum);
            numMonto.Value = valor;
        }

        private void SeleccionarMedioPago(string medio)
        {
            if (string.IsNullOrWhiteSpace(medio))
            {
                cboMedioPago.SelectedIndex = 0;
                return;
            }

            var idx = cboMedioPago.Items.IndexOf(medio);
            cboMedioPago.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private static string FormatearMonto(decimal monto) =>
            monto.ToString("C2", CultureInfo.CurrentCulture);

        private void LimpiarCampos()
        {
            txtDni.Clear();
            txtNombre.Clear();
            txtApellido.Clear();
            txtTelefono.Clear();
            numMonto.Value = 50m;
            cboMedioPago.SelectedIndex = 0;
        }

        private void CboActividad_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_sincronizandoActividad)
            {
                return;
            }

            ActualizarInfoCupoActividad();
        }

        private void RefrescarActividades(int? mantenerActividadId = null)
        {
            try
            {
                var idPrevio = mantenerActividadId ?? ObtenerActividadIdSeleccionada();
                _actividades = _actividadDao.ObtenerActivas().ToList();

                _sincronizandoActividad = true;
                cboActividad.BeginUpdate();
                cboActividad.DataSource = null;
                cboActividad.Items.Clear();
                foreach (var actividad in _actividades)
                {
                    cboActividad.Items.Add(actividad);
                }
                cboActividad.EndUpdate();

                if (idPrevio.HasValue && SeleccionarActividad(idPrevio.Value))
                {
                    // mantiene la selección del usuario
                }
                else if (cboActividad.Items.Count > 0)
                {
                    cboActividad.SelectedIndex = 0;
                }

                _sincronizandoActividad = false;
                ActualizarInfoCupoActividad();
            }
            catch (Exception ex)
            {
                _sincronizandoActividad = false;
                MessageBox.Show($"Error al cargar actividades: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? ObtenerActividadIdSeleccionada()
        {
            if (cboActividad.SelectedItem is Actividad actividad)
            {
                return actividad.IdActividad;
            }

            return null;
        }

        private bool SeleccionarActividad(int actividadId)
        {
            for (var i = 0; i < cboActividad.Items.Count; i++)
            {
                if (cboActividad.Items[i] is Actividad act && act.IdActividad == actividadId)
                {
                    cboActividad.SelectedIndex = i;
                    return true;
                }
            }

            return false;
        }

        private void SeleccionarPrimeraActividadConCupo()
        {
            var primera = _actividades.FirstOrDefault(a => a.TieneCupo);
            if (primera is not null)
            {
                SeleccionarActividad(primera.IdActividad);
            }
        }

        private bool ObtenerActividadSeleccionada(out Actividad actividad)
        {
            actividad = null!;
            if (cboActividad.SelectedItem is Actividad seleccionada)
            {
                actividad = seleccionada;
                return true;
            }

            MessageBox.Show("Seleccione una actividad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private bool ValidarCupoDisponible(int actividadId, int? excluirVisitanteId, out string nombre, out int ocupados, out int cupoMaximo)
        {
            nombre = string.Empty;
            ocupados = 0;
            cupoMaximo = 0;

            if (!_actividadDao.VerificarCupo(
                    actividadId,
                    excluirVisitanteId,
                    out var hayCupo,
                    out ocupados,
                    out cupoMaximo,
                    out nombre))
            {
                MessageBox.Show("No se pudo verificar el cupo de la actividad.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (!hayCupo)
            {
                InformarSinCupo(nombre, ocupados, cupoMaximo);
                return false;
            }

            return true;
        }

        private void InformarSinCupo(string nombreActividad, int ocupados, int cupoMaximo)
        {
            var mensaje =
                $"No hay cupo disponible en «{nombreActividad}» para hoy.\n\n" +
                $"Ocupados: {ocupados} de {cupoMaximo}.\n\n" +
                "E1: Se informa al visitante que la actividad está completa.";
            lblMensaje.Text = $"Sin cupo en {nombreActividad} ({ocupados}/{cupoMaximo}).";
            MessageBox.Show(mensaje, "Sin cupo en actividad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ActualizarInfoCupoActividad()
        {
            if (cboActividad.SelectedItem is not Actividad act)
            {
                lblCupoActividad.Text = string.Empty;
                return;
            }

            var hayCupo = act.TieneCupo;
            var ocupados = act.OcupadosHoy;
            var cupoMax = act.CupoMaximo;

            if (!_modoAlta && _visitanteSeleccionadoId.HasValue)
            {
                _actividadDao.VerificarCupo(act.IdActividad, _visitanteSeleccionadoId, out hayCupo, out ocupados, out cupoMax, out _);
            }

            if (!_modoAlta && hayCupo)
            {
                lblCupoActividad.Text = $"Cupo hoy: {ocupados}/{cupoMax} (este visitante conserva su lugar) — Tarifa: {act.PrecioVisitante:C2}";
                lblCupoActividad.ForeColor = UiTheme.TextoSecundario;
            }
            else if (hayCupo)
            {
                lblCupoActividad.Text = $"Cupo hoy: {ocupados}/{cupoMax} — Tarifa sugerida: {act.PrecioVisitante:C2}";
                lblCupoActividad.ForeColor = UiTheme.TextoSecundario;
            }
            else
            {
                lblCupoActividad.Text = $"SIN CUPO hoy ({ocupados}/{cupoMax})";
                lblCupoActividad.ForeColor = Color.DarkRed;
            }

            if (_modoAlta)
            {
                var valor = Math.Clamp(act.PrecioVisitante, numMonto.Minimum, numMonto.Maximum);
                numMonto.Value = valor;
            }
        }
    }
}

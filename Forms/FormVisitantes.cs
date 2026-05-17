using System.Drawing;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;
using TP_ClubDeportivo.Models;

namespace TP_ClubDeportivo.Forms
{
    public class FormVisitantes : Form
    {
        private readonly DataGridView dgvVisitantes;
        private readonly GroupBox grpFormulario;
        private readonly TextBox txtDni, txtNombre, txtApellido, txtTelefono, txtActividad, txtMonto;
        private readonly ComboBox cboMedioPago;
        private readonly Button btnGuardar;
        private readonly Button btnNuevo;
        private readonly Button btnEliminar;
        private readonly Button btnRegistrarPago;
        private readonly Label lblMensaje;
        private readonly Label lblModo;

        private readonly VisitanteDAO _visitanteDao = new();
        private readonly PagoDAO _pagoDao = new();

        private int? _visitanteSeleccionadoId;
        private bool _tienePagoRegistrado;

        public FormVisitantes()
        {
            Text = "Ingreso de Visitantes";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1000, 600);
            MinimumSize = new Size(900, 520);
            Font = UiTheme.FuenteNormal;
            BackColor = UiTheme.Fondo;

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 580,
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
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvVisitantes.SelectionChanged += DgvVisitantes_SelectionChanged;

            panelGrilla.Controls.Add(dgvVisitantes);
            panelGrilla.Controls.Add(btnRefrescar);
            split.Panel1.Controls.Add(panelGrilla);

            grpFormulario = new GroupBox
            {
                Text = "Nuevo visitante (CU-02)",
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            lblModo = new Label
            {
                Location = new Point(16, 4),
                Size = new Size(320, 20),
                ForeColor = UiTheme.Primario,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "Modo: alta de visitante"
            };
            grpFormulario.Controls.Add(lblModo);

            txtDni = CrearCampo(grpFormulario, "DNI:", 28);
            txtNombre = CrearCampo(grpFormulario, "Nombre:", 64);
            txtApellido = CrearCampo(grpFormulario, "Apellido:", 100);
            txtTelefono = CrearCampo(grpFormulario, "Teléfono:", 136);
            txtActividad = CrearCampo(grpFormulario, "Actividad:", 172);
            txtMonto = CrearCampo(grpFormulario, "Pago diario ($):", 208);
            txtMonto.Text = "50";
            UiTheme.AplicarCampo(txtMonto);

            grpFormulario.Controls.Add(new Label { Text = "Medio de pago:", Location = new Point(16, 248), AutoSize = true });
            cboMedioPago = new ComboBox
            {
                Location = new Point(130, 244),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMedioPago.Items.AddRange(["Efectivo", "Tarjeta Débito", "Tarjeta Crédito", "Transferencia"]);
            cboMedioPago.SelectedIndex = 0;
            grpFormulario.Controls.Add(cboMedioPago);

            btnGuardar = new Button
            {
                Text = "Registrar ingreso",
                Location = new Point(16, 288),
                Size = new Size(150, 36)
            };
            UiTheme.AplicarBotonPrimario(btnGuardar);
            btnGuardar.Click += BtnGuardar_Click;

            btnNuevo = new Button
            {
                Text = "Nuevo",
                Location = new Point(176, 288),
                Size = new Size(80, 36)
            };
            UiTheme.AplicarBotonSecundario(btnNuevo);
            btnNuevo.Click += (_, _) => ModoNuevo();

            btnEliminar = new Button
            {
                Text = "Eliminar",
                Location = new Point(264, 288),
                Size = new Size(80, 36),
                Enabled = false
            };
            UiTheme.AplicarBotonSecundario(btnEliminar);
            btnEliminar.Click += BtnEliminar_Click;

            btnRegistrarPago = new Button
            {
                Text = "Registrar pago",
                Location = new Point(16, 332),
                Size = new Size(150, 32),
                Enabled = false,
                Visible = false
            };
            UiTheme.AplicarBotonSecundario(btnRegistrarPago);
            btnRegistrarPago.Click += BtnRegistrarPago_Click;

            lblMensaje = new Label
            {
                Location = new Point(16, 372),
                Size = new Size(330, 80),
                ForeColor = UiTheme.TextoSecundario
            };

            grpFormulario.Controls.AddRange([
                btnGuardar, btnNuevo, btnEliminar, btnRegistrarPago, lblMensaje
            ]);

            split.Panel2.Controls.Add(grpFormulario);
            Controls.Add(split);

            Load += (_, _) =>
            {
                ModoNuevo();
                CargarVisitantes();
            };
        }

        private static TextBox CrearCampo(Control parent, string etiqueta, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = etiqueta,
                Location = new Point(16, y + 4),
                AutoSize = true,
                Font = UiTheme.FuenteNormal
            });
            var txt = new TextBox { Location = new Point(130, y), Size = new Size(214, 25) };
            UiTheme.AplicarCampo(txt);
            parent.Controls.Add(txt);
            return txt;
        }

        private void DgvVisitantes_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvVisitantes.CurrentRow?.DataBoundItem is not VisitanteListado fila)
            {
                return;
            }

            ModoEdicion(fila);
        }

        private void ModoEdicion(VisitanteListado fila)
        {
            _visitanteSeleccionadoId = fila.IdVisitante;
            _tienePagoRegistrado = fila.PagoRegistrado == "Sí";

            grpFormulario.Text = $"Editar visitante #{fila.IdVisitante}";
            lblModo.Text = "Modo: edición — seleccioná otro registro o «Nuevo» para alta";
            btnGuardar.Text = "Guardar cambios";
            btnEliminar.Enabled = true;

            txtDni.Text = fila.Dni;
            txtNombre.Text = fila.Nombre;
            txtApellido.Text = fila.Apellido;
            txtTelefono.Text = fila.Telefono;
            txtActividad.Text = fila.Actividad;
            txtMonto.Text = fila.Monto.ToString("N2");

            cboMedioPago.Enabled = !_tienePagoRegistrado;
            btnRegistrarPago.Visible = !_tienePagoRegistrado;
            btnRegistrarPago.Enabled = !_tienePagoRegistrado;

            if (_tienePagoRegistrado && !string.IsNullOrEmpty(fila.MedioPago))
            {
                var idx = cboMedioPago.Items.IndexOf(fila.MedioPago);
                cboMedioPago.SelectedIndex = idx >= 0 ? idx : 0;
            }

            lblMensaje.Text = _tienePagoRegistrado
                ? "Pago ya registrado. Podés actualizar datos del visitante."
                : "Sin pago registrado. Guardá cambios o usá «Registrar pago».";
        }

        private void ModoNuevo()
        {
            _visitanteSeleccionadoId = null;
            _tienePagoRegistrado = false;
            dgvVisitantes.ClearSelection();

            grpFormulario.Text = "Nuevo visitante (CU-02)";
            lblModo.Text = "Modo: alta de visitante";
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
            var idPrevio = _visitanteSeleccionadoId;

            try
            {
                dgvVisitantes.DataSource = _visitanteDao.ObtenerListado().ToList();
                ConfigurarColumnasGrilla();

                if (idPrevio.HasValue)
                {
                    foreach (DataGridViewRow row in dgvVisitantes.Rows)
                    {
                        if (row.DataBoundItem is VisitanteListado v && v.IdVisitante == idPrevio.Value)
                        {
                            row.Selected = true;
                            break;
                        }
                    }
                }
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

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            if (!ValidarCampos(out var monto))
            {
                return;
            }

            try
            {
                if (_visitanteSeleccionadoId.HasValue)
                {
                    ActualizarVisitante(monto);
                }
                else
                {
                    CrearVisitante(monto);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearVisitante(decimal monto)
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

            ModoNuevo();
            CargarVisitantes();
        }

        private void ActualizarVisitante(decimal monto)
        {
            var visitante = new Visitante
            {
                IdVisitante = _visitanteSeleccionadoId!.Value,
                DNI = txtDni.Text.Trim(),
                Nombre = txtNombre.Text.Trim(),
                Apellido = txtApellido.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                Actividad = txtActividad.Text.Trim(),
                PagoDiarioMonto = monto
            };

            if (!_visitanteDao.Actualizar(visitante))
            {
                lblMensaje.Text = "No se pudieron guardar los cambios.";
                return;
            }

            lblMensaje.Text = $"Visitante #{visitante.IdVisitante} actualizado.";
            MessageBox.Show("Datos del visitante actualizados.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            var concepto = $"Entrada diaria — {txtActividad.Text.Trim()}";
            if (!_pagoDao.RegistrarPagoVisitante(_visitanteSeleccionadoId.Value, monto, cboMedioPago.Text, concepto, out var pagoId))
            {
                MessageBox.Show("No se pudo registrar el pago.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show($"Pago #{pagoId} registrado por ${monto:N2}.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            CargarVisitantes();
        }

        private void BtnEliminar_Click(object? sender, EventArgs e)
        {
            if (!_visitanteSeleccionadoId.HasValue)
            {
                return;
            }

            var textoConfirmacion = _tienePagoRegistrado
                ? $"¿Eliminar al visitante #{_visitanteSeleccionadoId.Value}?\n\n" +
                  "Tiene pagos diarios registrados. También se eliminarán esos pagos."
                : $"¿Eliminar al visitante #{_visitanteSeleccionadoId.Value}?";

            var confirmar = MessageBox.Show(
                textoConfirmacion,
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmar != DialogResult.Yes)
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

        private bool ValidarCampos(out decimal monto)
        {
            monto = 0;
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtActividad.Text))
            {
                MessageBox.Show("Complete al menos nombre y actividad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!decimal.TryParse(txtMonto.Text.Trim(), out monto) || monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            txtActividad.Clear();
            txtMonto.Text = "50";
            cboMedioPago.SelectedIndex = 0;
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TP_ClubDeportivo;
using TP_ClubDeportivo.DAO;

namespace TP_ClubDeportivo.Forms
{
    public class FormLogin : Form
    {
        private static readonly string ArchivoUsuarioRecordado = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TP_ClubDeportivo",
            "usuario.txt");

        private readonly TextBox txtUsername;
        private readonly TextBox txtPassword;
        private readonly Button btnLogin;
        private readonly Button btnTestConexion;
        private readonly Button btnTogglePassword;
        private readonly CheckBox chkRecordarUsuario;
        private readonly Label lblError;
        private readonly Panel panelCard;

        public FormLogin()
        {
            Text = "Club Deportivo";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(820, 480);
            MinimumSize = new Size(820, 480);
            BackColor = UiTheme.Fondo;
            Font = UiTheme.FuenteNormal;
            DoubleBuffered = true;

            var panelIzquierdo = new Panel
            {
                Dock = DockStyle.Left,
                Width = 340,
                BackColor = UiTheme.Primario
            };
            panelIzquierdo.Paint += PanelIzquierdo_Paint;

            var lblMarca = new Label
            {
                Text = "Club Deportivo",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(32, 48)
            };

            var lblTagline = new Label
            {
                Text = "Sistema de gestión integral\nSocios · Cuotas · Personal · Nutrición",
                ForeColor = Color.FromArgb(220, 235, 255),
                Font = new Font("Segoe UI", 10.5F),
                AutoSize = true,
                Location = new Point(32, 100)
            };

            var lblVersion = new Label
            {
                Text = "v1.0 — TP Desarrollo de Sistemas",
                ForeColor = Color.FromArgb(180, 210, 245),
                Font = new Font("Segoe UI", 8.5F),
                AutoSize = true,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(32, 420)
            };

            panelIzquierdo.Controls.AddRange([lblMarca, lblTagline, lblVersion]);

            panelCard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(48, 40, 48, 24),
                BackColor = UiTheme.Fondo
            };

            var lblTitulo = new Label
            {
                Text = "Iniciar sesión",
                Font = UiTheme.FuenteTitulo,
                ForeColor = UiTheme.Texto,
                AutoSize = true,
                Location = new Point(0, 0)
            };

            var lblSubtitulo = new Label
            {
                Text = "Ingrese sus credenciales para acceder al sistema.",
                Font = UiTheme.FuenteSubtitulo,
                ForeColor = UiTheme.TextoSecundario,
                AutoSize = true,
                Location = new Point(0, 38)
            };

            var lblUsuario = new Label
            {
                Text = "Usuario",
                ForeColor = UiTheme.Texto,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 88)
            };

            txtUsername = new TextBox
            {
                Location = new Point(0, 110),
                Size = new Size(380, 32),
                PlaceholderText = "Ej: admin"
            };
            UiTheme.AplicarCampo(txtUsername);

            var lblPassword = new Label
            {
                Text = "Contraseña",
                ForeColor = UiTheme.Texto,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 158)
            };

            txtPassword = new TextBox
            {
                Location = new Point(0, 180),
                Size = new Size(328, 32),
                UseSystemPasswordChar = true,
                PlaceholderText = "Ingrese su contraseña"
            };
            UiTheme.AplicarCampo(txtPassword);

            btnTogglePassword = new Button
            {
                Text = "Ver",
                Location = new Point(336, 180),
                Size = new Size(84, 32),
                TabStop = false
            };
            UiTheme.AplicarBotonSecundario(btnTogglePassword);
            btnTogglePassword.Click += BtnTogglePassword_Click;

            chkRecordarUsuario = new CheckBox
            {
                Text = "Recordar usuario",
                Location = new Point(0, 224),
                AutoSize = true,
                ForeColor = UiTheme.TextoSecundario
            };

            lblError = new Label
            {
                Location = new Point(0, 252),
                Size = new Size(420, 40),
                ForeColor = UiTheme.Error,
                Visible = false
            };

            btnLogin = new Button
            {
                Text = "Ingresar",
                Location = new Point(0, 300),
                Size = new Size(420, 44)
            };
            UiTheme.AplicarBotonPrimario(btnLogin);
            btnLogin.Click += BtnLogin_Click;

            btnTestConexion = new Button
            {
                Text = "Probar conexión a la base de datos",
                Location = new Point(0, 356),
                Size = new Size(420, 36)
            };
            UiTheme.AplicarBotonSecundario(btnTestConexion);
            btnTestConexion.Click += BtnTestConexion_Click;

            panelCard.Controls.AddRange([
                lblTitulo, lblSubtitulo, lblUsuario, txtUsername,
                lblPassword, txtPassword, btnTogglePassword,
                chkRecordarUsuario, lblError, btnLogin, btnTestConexion
            ]);

            Controls.Add(panelCard);
            Controls.Add(panelIzquierdo);

            AcceptButton = btnLogin;
            txtUsername.KeyDown += Campo_KeyDown;
            txtPassword.KeyDown += Campo_KeyDown;

            Shown += (_, _) =>
            {
                CargarUsuarioRecordado();
                txtUsername.Focus();
                if (string.IsNullOrEmpty(txtUsername.Text))
                {
                    txtUsername.Select();
                }
                else
                {
                    txtPassword.Focus();
                }
            };
        }

        private static void PanelIzquierdo_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
            {
                return;
            }

            using var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                UiTheme.Primario,
                UiTheme.PrimarioOscuro,
                LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, panel.ClientRectangle);

            using var circulo = new SolidBrush(Color.FromArgb(30, 255, 255, 255));
            e.Graphics.FillEllipse(circulo, panel.Width - 120, panel.Height - 140, 200, 200);
            e.Graphics.FillEllipse(circulo, -60, 200, 160, 160);
        }

        private void Campo_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnLogin_Click(btnLogin, EventArgs.Empty);
            }
        }

        private void BtnTogglePassword_Click(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            btnTogglePassword.Text = txtPassword.UseSystemPasswordChar ? "Ver" : "Ocultar";
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            OcultarError();

            var usuario = txtUsername.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario))
            {
                MostrarError("Ingrese su nombre de usuario.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MostrarError("Ingrese su contraseña.");
                txtPassword.Focus();
                return;
            }

            EstablecerCargando(true);

            try
            {
                var resultado = new UsuarioDAO().Login(usuario, password);
                if (resultado is null)
                {
                    MostrarError("Usuario o contraseña incorrectos. Verifique sus datos e intente nuevamente.");
                    txtPassword.SelectAll();
                    txtPassword.Focus();
                    return;
                }

                GuardarUsuarioRecordado(usuario);

                Sesion.Iniciar(resultado);

                if (!Permisos.PuedeIngresarAlSistema())
                {
                    var rol = resultado.Rol;
                    using var accesoDenegado = new FormAccesoDenegado(
                        $"El rol «{rol}» no puede usar la aplicación de gestión del club.\n\n" +
                        "Esta versión está destinada a personal interno: Administrador, Empleado, Profesor o Nutricionista.\n\n" +
                        "Los usuarios Socio y Visitante no tienen acceso a este módulo.");
                    accesoDenegado.ShowDialog();
                    Sesion.Cerrar();
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                Hide();
                using var principal = new FormPrincipal();
                principal.ShowDialog();
                Sesion.Cerrar();

                txtPassword.Clear();
                OcultarError();
                Show();
                txtPassword.Focus();
            }
            catch (Exception ex)
            {
                MostrarError($"No se pudo conectar al sistema.\n{ex.Message}");
            }
            finally
            {
                EstablecerCargando(false);
            }
        }

        private void BtnTestConexion_Click(object? sender, EventArgs e)
        {
            OcultarError();
            EstablecerCargando(true);

            try
            {
                var ok = new UsuarioDAO().TestConexion();
                MostrarMensaje(
                    ok ? "Conexión a la base de datos establecida correctamente." : "No se pudo conectar. Verifique que MySQL esté en ejecución.",
                    ok);
            }
            catch (Exception ex)
            {
                MostrarError($"Error de conexión: {ex.Message}");
            }
            finally
            {
                EstablecerCargando(false);
            }
        }

        private void EstablecerCargando(bool cargando)
        {
            btnLogin.Enabled = !cargando;
            btnTestConexion.Enabled = !cargando;
            txtUsername.Enabled = !cargando;
            txtPassword.Enabled = !cargando;
            btnTogglePassword.Enabled = !cargando;
            chkRecordarUsuario.Enabled = !cargando;
            btnLogin.Text = cargando ? "Ingresando…" : "Ingresar";
            Cursor = cargando ? Cursors.WaitCursor : Cursors.Default;
        }

        private void MostrarError(string mensaje)
        {
            lblError.Text = mensaje;
            lblError.ForeColor = UiTheme.Error;
            lblError.Visible = true;
        }

        private void MostrarMensaje(string mensaje, bool exito)
        {
            lblError.Text = mensaje;
            lblError.ForeColor = exito ? UiTheme.Exito : UiTheme.Error;
            lblError.Visible = true;
        }

        private void OcultarError() => lblError.Visible = false;

        private void CargarUsuarioRecordado()
        {
            try
            {
                if (!File.Exists(ArchivoUsuarioRecordado))
                {
                    return;
                }

                var usuario = File.ReadAllText(ArchivoUsuarioRecordado).Trim();
                if (!string.IsNullOrEmpty(usuario))
                {
                    txtUsername.Text = usuario;
                    chkRecordarUsuario.Checked = true;
                }
            }
            catch
            {
                // Ignorar errores de lectura local
            }
        }

        private void GuardarUsuarioRecordado(string usuario)
        {
            try
            {
                if (chkRecordarUsuario.Checked)
                {
                    var dir = Path.GetDirectoryName(ArchivoUsuarioRecordado)!;
                    Directory.CreateDirectory(dir);
                    File.WriteAllText(ArchivoUsuarioRecordado, usuario);
                }
                else if (File.Exists(ArchivoUsuarioRecordado))
                {
                    File.Delete(ArchivoUsuarioRecordado);
                }
            }
            catch
            {
                // Ignorar errores de escritura local
            }
        }
    }
}

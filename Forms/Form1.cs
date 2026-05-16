using System.Data;
using System.Drawing;
using System.Windows.Forms;
using TP_ClubDeportivo.DAO;

namespace TP_ClubDeportivo.Forms
{
    public class Form1 : Form
    {
        private readonly Label lblUsername;
        private readonly TextBox txtUsername;
        private readonly Label lblPassword;
        private readonly TextBox txtPassword;
        private readonly Button btnLogin;
        private readonly Button btnTestConexion;
        private readonly Label lblStatus;

        public Form1()
        {
            Text = "TP Club Deportivo - Login";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(420, 240);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

            lblUsername = new Label
            {
                Text = "Usuario:",
                Location = new Point(24, 24),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Location = new Point(130, 20),
                Size = new Size(250, 25)
            };

            lblPassword = new Label
            {
                Text = "Contraseña:",
                Location = new Point(24, 64),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Location = new Point(130, 60),
                Size = new Size(250, 25),
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "Ingresar",
                Location = new Point(24, 110),
                Size = new Size(150, 36)
            };
            btnLogin.Click += BtnLogin_Click;

            btnTestConexion = new Button
            {
                Text = "Probar conexión",
                Location = new Point(190, 110),
                Size = new Size(150, 36)
            };
            btnTestConexion.Click += BtnTestConexion_Click;

            lblStatus = new Label
            {
                Text = "Ingrese sus credenciales y presione Ingresar.",
                Location = new Point(24, 160),
                Size = new Size(372, 60),
                AutoSize = false
            };

            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
            Controls.Add(btnTestConexion);
            Controls.Add(lblStatus);
        }

        private void BtnLogin_Click(object? sender, System.EventArgs e)
        {
            try
            {
                var usuarioDao = new UsuarioDAO();
                var usuario = usuarioDao.Login(txtUsername.Text.Trim(), txtPassword.Text.Trim());

                if (usuario is null)
                {
                    lblStatus.Text = "Usuario o contraseña incorrectos.";
                }
                else
                {
                    lblStatus.Text = $"Login OK. Bienvenido {usuario.Username}. Rol: {usuario.Rol}";
                }
            }
            catch (System.Exception ex)
            {
                lblStatus.Text = $"Error de login: {ex.Message}";
            }
        }

        private void BtnTestConexion_Click(object? sender, System.EventArgs e)
        {
            try
            {
                var usuarioDao = new UsuarioDAO();
                lblStatus.Text = usuarioDao.TestConexion()
                    ? "Conexión a la base de datos OK."
                    : "No se pudo conectar a la base de datos.";
            }
            catch (System.Exception ex)
            {
                lblStatus.Text = $"Error de conexión: {ex.Message}";
            }
        }
    }
}

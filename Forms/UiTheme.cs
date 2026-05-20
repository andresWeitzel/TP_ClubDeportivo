using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TP_ClubDeportivo.Forms
{
    internal static class UiTheme
    {
        public static readonly Color Primario = Color.FromArgb(37, 99, 168);
        public static readonly Color PrimarioOscuro = Color.FromArgb(28, 76, 128);
        public static readonly Color PrimarioClaro = Color.FromArgb(232, 240, 252);
        public static readonly Color Fondo = Color.FromArgb(245, 247, 250);
        public static readonly Color Tarjeta = Color.White;
        public static readonly Color Texto = Color.FromArgb(33, 37, 41);
        public static readonly Color TextoSecundario = Color.FromArgb(108, 117, 125);
        public static readonly Color Error = Color.FromArgb(185, 28, 28);
        public static readonly Color Exito = Color.FromArgb(22, 120, 68);
        public static readonly Color Borde = Color.FromArgb(222, 226, 230);
        public static readonly Color SidebarHover = Color.FromArgb(48, 115, 185);

        public static Font FuenteTitulo => new("Segoe UI", 18F, FontStyle.Bold);
        public static Font FuenteSubtitulo => new("Segoe UI", 10F);
        public static Font FuenteNormal => new("Segoe UI", 10F);

        public static void AplicarBotonPrimario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = Primario;
            boton.ForeColor = Color.White;
            boton.Cursor = Cursors.Hand;
            boton.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            boton.FlatAppearance.MouseOverBackColor = PrimarioOscuro;
        }

        public static void AplicarBotonSecundario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = Borde;
            boton.BackColor = Tarjeta;
            boton.ForeColor = Texto;
            boton.Cursor = Cursors.Hand;
        }

        public static void AplicarCampo(TextBox campo)
        {
            campo.BorderStyle = BorderStyle.FixedSingle;
            campo.BackColor = Color.White;
            campo.Font = FuenteNormal;
        }

        public static Panel CrearTarjetaAcceso(string titulo, string descripcion, string modulo, Action alHacerClic)
        {
            var tarjeta = new Panel
            {
                Size = new Size(272, 158),
                MinimumSize = new Size(240, 150),
                BackColor = Tarjeta,
                Cursor = Cursors.Hand,
                Margin = new Padding(16, 16, 16, 16),
                Padding = new Padding(20, 18, 16, 16)
            };

            tarjeta.Paint += (_, e) =>
            {
                var rect = new Rectangle(0, 0, tarjeta.Width - 1, tarjeta.Height - 1);
                using var pen = new Pen(Borde);
                e.Graphics.DrawRectangle(pen, rect);
                using var accent = new SolidBrush(Primario);
                e.Graphics.FillRectangle(accent, 0, 0, 4, tarjeta.Height);
            };

            var lblModulo = new Label
            {
                Text = modulo.ToUpperInvariant(),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Primario,
                AutoSize = true,
                Location = new Point(16, 16),
                BackColor = Color.Transparent
            };

            var lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Texto,
                AutoSize = true,
                Location = new Point(16, 38),
                BackColor = Color.Transparent
            };

            var lblDesc = new Label
            {
                Text = descripcion,
                Font = FuenteSubtitulo,
                ForeColor = TextoSecundario,
                Size = new Size(228, 52),
                Location = new Point(16, 66),
                BackColor = Color.Transparent
            };

            var lblIr = new Label
            {
                Text = "Abrir →",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Primario,
                AutoSize = true,
                Location = new Point(16, 124),
                BackColor = Color.Transparent
            };

            tarjeta.Controls.AddRange([lblModulo, lblTitulo, lblDesc, lblIr]);

            void ActivarHover(object? s, EventArgs e) => tarjeta.BackColor = PrimarioClaro;
            void DesactivarHover(object? s, EventArgs e) => tarjeta.BackColor = Tarjeta;
            void Click(object? s, EventArgs e) => alHacerClic();

            tarjeta.MouseEnter += ActivarHover;
            tarjeta.MouseLeave += DesactivarHover;
            tarjeta.Click += Click;
            foreach (Control c in tarjeta.Controls)
            {
                c.MouseEnter += ActivarHover;
                c.MouseLeave += DesactivarHover;
                c.Click += Click;
            }

            return tarjeta;
        }

        public static Button CrearBotonSidebar(string texto, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = "  " + texto,
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                BackColor = Primario,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 0, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = SidebarHover;
            btn.Click += onClick;
            return btn;
        }

        public static void ConfigurarSplitVertical(
            SplitContainer split,
            double ratioPanel1 = 0.58,
            int panel1Min = 260,
            int panel2Min = 260)
        {
            split.SplitterWidth = 4;

            void Ajustar()
            {
                if (split.Width <= 0 || split.IsDisposed)
                {
                    return;
                }

                var anchoMinimoTotal = panel1Min + panel2Min + split.SplitterWidth;
                if (split.Width < anchoMinimoTotal)
                {
                    var tercio = Math.Max(80, (split.Width - split.SplitterWidth) / 3);
                    split.Panel1MinSize = tercio;
                    split.Panel2MinSize = tercio;
                }
                else
                {
                    split.Panel1MinSize = panel1Min;
                    split.Panel2MinSize = panel2Min;
                }

                var maximo = split.Width - split.Panel2MinSize - split.SplitterWidth;
                var minimo = split.Panel1MinSize;
                if (maximo < minimo)
                {
                    return;
                }

                var deseado = (int)(split.Width * ratioPanel1);
                try
                {
                    split.SplitterDistance = Math.Clamp(deseado, minimo, maximo);
                }
                catch (InvalidOperationException)
                {
                    // El control aún no tiene tamaño definitivo; se reintenta en el próximo Resize.
                }
            }

            split.Resize += (_, _) => Ajustar();
            split.HandleCreated += (_, _) => Ajustar();
            if (split.IsHandleCreated)
            {
                Ajustar();
            }
        }

        public static void PintarFondoGradiente(Panel panel, PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                panel.ClientRectangle,
                Primario,
                PrimarioOscuro,
                LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, panel.ClientRectangle);
        }
    }
}

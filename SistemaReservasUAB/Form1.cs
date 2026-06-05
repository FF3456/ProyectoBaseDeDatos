using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaReservasUAB
{

    public partial class Form1 : Form
    {
        public string UsuarioActual { get; set; }

        public string RolActual { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        public void RefrescarEstado()
        {
            ActualizarBarraEstado();
        }
        private void OpenFormInPanel(Form frm)
        {
            this.panelContenedor.Controls.Clear();
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            this.panelContenedor.Controls.Add(frm);
            this.panelContenedor.Tag = frm;
            frm.Show();
        }

        private void buttonUsuarios_Click(object sender, EventArgs e)
        {
            OpenFormInPanel(new FrmUsuarios());
        }

        public void ActualizarUsuario()
        {
            lblUsuario.Text = "Usuario: " + UsuarioActual;
            lblRol.Text = "Rol: " + RolActual;
            // Configurar accesos segun el rol
            ConfigureAccessByRole();
        }

        private void ConfigureAccessByRole()
        {
            // Controles del sidebar:
            // button2 = Usuarios
            // button3 = Ambientes
            // button4 = Reservas
            // button5 = Eventos
            // button6 = Reportes

            // Ocultar todo por defecto
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;

            if (string.IsNullOrWhiteSpace(RolActual))
                return;

            string rol = RolActual.Trim().ToLower();

            // Reglas de acceso por rol (ajustables):
            // Administrador: acceso completo
            // Docente: Ambientes, Reservas, Eventos, Reportes
            // Estudiante: Reservas

            if (rol.Contains("administrador") || rol.Contains("admin") || rol == "adm")
            {
                button2.Visible = true; // Usuarios
                button3.Visible = true; // Ambientes
                button4.Visible = true; // Reservas
                button5.Visible = true; // Eventos
                button6.Visible = true; // Reportes
                return;
            }

            if (rol.Contains("docente") || rol == "doc")
            {
                button3.Visible = true; // Ambientes
                button4.Visible = true; // Reservas
                button5.Visible = true; // Eventos
                button6.Visible = true; // Reportes
                return;
            }

            if (rol.Contains("estudiante") || rol == "est")
            {
                button4.Visible = true; // Reservas
                return;
            }
        }
        private void buttonAmbientes_Click(object sender, EventArgs e)
        {
            OpenFormInPanel(new FrmAmbientes());
        }

        private void buttonReservas_Click(object sender, EventArgs e)
        {
            OpenFormInPanel(new FrmReservas());
        }

        private void buttonEventos_Click(object sender, EventArgs e)
        {
            OpenFormInPanel(new FrmEventos());
        }

        private void buttonReportes_Click(object sender, EventArgs e)
        {
            OpenFormInPanel(new FrmReportes());
        }

        private void buttonSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

        private void panelSidebar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ActualizarBarraEstado()
        {
            lblUsuario.Text = $"Usuario: {UsuarioActual}";
            lblRol.Text = $"Rol: {RolActual}";

            lblEstado.Text =
                "Estado: Listo";

            lblFecha.Text =
                "Fecha: " + DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void panelContenedor_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timerFechaHora_Tick(
    object sender,
    EventArgs e)
        {
            lblFecha.Text =
                DateTime.Now.ToString(
                    "dd/MM/yyyy HH:mm:ss");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timerFechaHora.Start();

            lblEstado.Text = "Sistema iniciado";

            ActualizarUsuario();

            lblFecha.Text =
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }
    }
}

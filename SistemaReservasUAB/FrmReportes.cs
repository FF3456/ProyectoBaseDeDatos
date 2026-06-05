using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
// include for embedding forms
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaReservasUAB
{
    public partial class FrmReportes : Form
    {
        public FrmReportes()
        {
            InitializeComponent();
            this.tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            this.btnGenerarReportes.Click += BtnGenerarReportes_Click;
            this.comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;

            CargarCarreras();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When Disponibilidad tab is selected, embed FrmDisponibilidad
            if (this.tabControl1.SelectedTab != null && this.tabControl1.SelectedTab.Text == "Disponibilidad")
            {
                // avoid adding multiple instances
                if (!this.tabPage7.Controls.OfType<FrmDisponibilidad>().Any())
                {
                    var frm = new FrmDisponibilidad();
                    frm.TopLevel = false;
                    frm.FormBorderStyle = FormBorderStyle.None;
                    frm.Dock = DockStyle.Fill;
                    this.tabPage7.Controls.Clear();
                    this.tabPage7.Controls.Add(frm);
                    frm.Show();
                }
            }
        }

        private void BtnGenerarReportes_Click(object sender, EventArgs e)
        {
            DateTime inicio = dtpFechaInicio.Value.Date;
            DateTime fin = dtpFechaFin.Value.Date.AddDays(1).AddSeconds(-1);

            try
            {
                string conn = Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (var cn = new System.Data.SqlClient.SqlConnection(conn))
                {
                    cn.Open();

                    // =====================================================
                    // TAB 1 - AMBIENTES MÁS UTILIZADOS (usar vista vw_reserva_ambiente_detail)
                    // =====================================================
                    try
                    {
                        using (var cmd = new SqlCommand(@"
SELECT
    codigo_ambiente,
    nombre_bloque,
    tipo_ambiente,
    capacidad_maxima,
    COUNT(*) AS TotalReservas

FROM vw_reserva_detalle

WHERE fecha_registro BETWEEN @ini AND @fin

GROUP BY
    codigo_ambiente,
    nombre_bloque,
    tipo_ambiente,
    capacidad_maxima

ORDER BY TotalReservas DESC", cn))
                        {
                            cmd.Parameters.AddWithValue("@ini", inicio);
                            cmd.Parameters.AddWithValue("@fin", fin);

                            DataTable dt = new DataTable();

                            new SqlDataAdapter(cmd).Fill(dt);

                            dataGridView1.DataSource = dt;
                        }
                    }
                    catch { dataGridView1.DataSource = null; }

                    // =====================================================
                    // TAB 2 - HORARIOS MÁS OCUPADOS (usar vista vw_horarios_ocupados)
                    // =====================================================
                    try
                    {
                        using (var cmd = new SqlCommand(@"
SELECT *
FROM vw_horarios_ocupados
WHERE fecha_especifica BETWEEN @ini AND @fin
ORDER BY total_reservas DESC", cn))
                        {
                            cmd.Parameters.AddWithValue("@ini", inicio);
                            cmd.Parameters.AddWithValue("@fin", fin);

                            DataTable dt = new DataTable();

                            new SqlDataAdapter(cmd).Fill(dt);

                            dataGridView2.DataSource = dt;
                        }
                    }
                    catch { dataGridView2.DataSource = null; }

                    // =====================================================
                    // TAB 3 - USO POR CARRERA (usar vista vw_uso_por_carrera)
                    // =====================================================
                    try
                    {
                        string carrera = comboBox1.Text;
                        string sql =
@"SELECT *
FROM vw_uso_por_carrera";

                        if (carrera != "Todas")
                        {
                            sql += " WHERE carrera = @carrera";
                        }

                        sql += " ORDER BY total_reservas DESC";

                        using (var cmd = new SqlCommand(sql, cn))
                        {
                            if (carrera != "Todas")
                                cmd.Parameters.AddWithValue("@carrera", carrera);

                            DataTable dt = new DataTable();

                            new SqlDataAdapter(cmd).Fill(dt);

                            dataGridView3.DataSource = dt;
                        }
                    }
                    catch { dataGridView3.DataSource = null; }

                    // =====================================================
                    // TAB 4 - USO POR TIPO DE USUARIO (usar vista vw_uso_por_tipo_usuario)
                    // =====================================================
                    try
                    {
                        using (var cmd = new SqlCommand(@"
SELECT *
FROM vw_uso_por_tipo_usuario
ORDER BY total_reservas DESC", cn))
                        {
                            DataTable dt = new DataTable();

                            new SqlDataAdapter(cmd).Fill(dt);

                            dataGridView4.DataSource = dt;
                        }
                    }
                    catch { dataGridView4.DataSource = null; }

                    // =====================================================
                    // TAB 5 - RESERVAS CANCELADAS (usar vista vw_reservas_canceladas)
                    // =====================================================
                    try
                    {
                        using (var cmd = new SqlCommand(@"
SELECT *
FROM vw_reservas_canceladas
WHERE fecha_registro BETWEEN @ini AND @fin
ORDER BY fecha_registro DESC", cn))
                        {
                            cmd.Parameters.AddWithValue("@ini", inicio);
                            cmd.Parameters.AddWithValue("@fin", fin);

                            DataTable dt = new DataTable();

                            new SqlDataAdapter(cmd).Fill(dt);

                            dataGridView5.DataSource = dt;
                        }
                    }
                    catch { dataGridView5.DataSource = null; }

                    // TAB DISPONIBILIDAD
                    if (this.tabPage7.Controls
                        .OfType<FrmDisponibilidad>()
                        .FirstOrDefault() is FrmDisponibilidad fd)
                    {
                        fd.LoadDisponibilidad();
                    }

                    cn.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al generar reportes:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void CargarCarreras()
        {
            try
            {
                string conn = Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (var cn = new System.Data.SqlClient.SqlConnection(conn))
                {
                    cn.Open();

                    comboBox1.Items.Clear();

                    comboBox1.Items.Add("Todas");

                    using (var cmd = new System.Data.SqlClient.SqlCommand(
                        "SELECT DISTINCT carrera FROM ESTUDIANTE ORDER BY carrera", cn))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                comboBox1.Items.Add(dr["carrera"].ToString());
                            }
                        }
                    }

                    comboBox1.SelectedIndex = 0;
                }
            }
            catch
            {
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BtnGenerarReportes_Click(sender, e);
        }
    }

}

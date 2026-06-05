using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace SistemaReservasUAB
{
    public partial class FrmAmbientes : Form
    {
        private DataTable dtAmbientes;
        private string idColumnName;
        private int? selectedId;

        public FrmAmbientes()
        {
            InitializeComponent();
            this.Load += FrmAmbientes_Load;
            this.btnBuscar.Click += btnBuscar_Click;
            this.btnNuevo.Click += btnNuevo_Click;
            this.btnLimpiar.Click += btnLimpiar_Click;
            this.btnGuardar.Click += btnGuardar_Click;
            this.btnEliminar.Click += btnEliminar_Click;
            this.dgvUsuarios.CellClick += dgvUsuarios_CellClick;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void FrmAmbientes_Load(object sender, EventArgs e)
        {
            CargarCombos();
            LoadAmbientes();
        }



        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LoadAmbientes(txtBuscar.Text.Trim());
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void dgvUsuarios_CellClick(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow fila =
                dgvUsuarios.Rows[e.RowIndex];

            selectedId =
                Convert.ToInt32(
                    fila.Cells["id_ambiente"].Value);

            textBox1.Text =
                fila.Cells["codigo_ambiente"].Value.ToString();

            comboBox1.Text =
                fila.Cells["nombre_bloque"].Value.ToString();

            comboBox2.Text =
                fila.Cells["nombre_tipo"].Value.ToString();

            comboBox3.Text =
                fila.Cells["nombre_estado"].Value.ToString();

            numericUpDown1.Value =
                Convert.ToDecimal(
                    fila.Cells["capacidad_maxima"].Value);
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            SaveAmbiente();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            EditarAmbiente();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                MessageBox.Show("Seleccione un ambiente para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("¿Eliminar el ambiente seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteAmbiente();
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            textBox1.Clear();
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            numericUpDown1.Value = 0;
            for (int i = 0; i < clbCaracteristicas.Items.Count; i++) clbCaracteristicas.SetItemChecked(i, false);
        }

        private void LoadAmbientes(string filtro = "")
        {
            try
            {
                string conn =
                    Properties.Settings.Default.SistemaReservasUABConnectionString;

                string sql = @"
SELECT
    a.id_ambiente,
    a.codigo_ambiente,
    b.nombre_bloque,
    ta.nombre_tipo,
    ea.nombre_estado,
    a.capacidad_maxima
FROM AMBIENTE a
INNER JOIN BLOQUE b
    ON a.id_bloque = b.id_bloque
INNER JOIN TIPO_AMBIENTE ta
    ON a.id_tipo_ambiente = ta.id_tipo_ambiente
INNER JOIN ESTADO_AMBIENTE ea
    ON a.id_estado_ambiente = ea.id_estado_ambiente
WHERE
    a.codigo_ambiente LIKE @f
    OR b.nombre_bloque LIKE @f
    OR ta.nombre_tipo LIKE @f";

                using (SqlConnection cn = new SqlConnection(conn))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, cn))
                {
                    da.SelectCommand.Parameters.AddWithValue(
                        "@f",
                        "%" + filtro + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvUsuarios.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetIdColumnName()
        {
            if (dtAmbientes == null || dtAmbientes.Columns.Count == 0) return "";
            var col = dtAmbientes.Columns.Cast<DataColumn>().FirstOrDefault(c => c.ColumnName.ToLower().Contains("id"));
            if (col != null) return col.ColumnName;
            return dtAmbientes.Columns[0].ColumnName;
        }

        private string FindColumn(params string[] candidates)
        {
            if (dtAmbientes == null) return null;
            foreach (var c in dtAmbientes.Columns.Cast<DataColumn>())
            {
                string name = c.ColumnName.ToLower();
                foreach (var cand in candidates)
                {
                    if (name.Contains(cand.ToLower())) return c.ColumnName;
                }
            }
            return null;
        }

        private string GetRowString(DataRow row, string[] candidates)
        {
            var col = FindColumn(candidates);
            if (col == null) return string.Empty;
            var val = row[col];
            return val == null ? string.Empty : val.ToString();
        }

        private void SaveAmbiente()
        {
            try
            {
                string conn =
                    Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (SqlConnection cn = new SqlConnection(conn))
                {
                    string sql = @"
INSERT INTO AMBIENTE
(
    codigo_ambiente,
    id_bloque,
    id_tipo_ambiente,
    id_estado_ambiente,
    capacidad_maxima
)
VALUES
(
    @codigo,
    @bloque,
    @tipo,
    @estado,
    @capacidad
)";

                    SqlCommand cmd =
                        new SqlCommand(sql, cn);

                    cmd.Parameters.AddWithValue(
                        "@codigo",
                        textBox1.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@bloque",
                        comboBox1.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@tipo",
                        comboBox2.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@estado",
                        comboBox3.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@capacidad",
                        numericUpDown1.Value);

                    cn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Ambiente registrado");
                }

                LoadAmbientes();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteAmbiente()
        {
            if (dtAmbientes == null || selectedId == null) return;
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;
            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"DELETE FROM ambiente WHERE [{idColumnName}] = @id";
                    cmd.Parameters.AddWithValue("@id", selectedId.Value);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    LoadAmbientes();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar ambiente: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivarAmbiente()
        {
            if (selectedId == null)
                return;

            string conn =
                Properties.Settings.Default.SistemaReservasUABConnectionString;

            using (SqlConnection cn = new SqlConnection(conn))
            {
                SqlCommand cmd =
                    new SqlCommand(
                        @"UPDATE AMBIENTE
                  SET id_estado_ambiente = 1
                  WHERE id_ambiente=@id",
                        cn);

                cmd.Parameters.AddWithValue(
                    "@id",
                    selectedId);

                cn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadAmbientes();
        }

        private void CargarCombos()
        {
            string conn = Properties.Settings.Default.SistemaReservasUABConnectionString;

            using (SqlConnection cn = new SqlConnection(conn))
            {
                cn.Open();

                // BLOQUES
                SqlDataAdapter daBloque =
                    new SqlDataAdapter(
                        "SELECT id_bloque,nombre_bloque FROM BLOQUE",
                        cn);

                DataTable dtBloque = new DataTable();
                daBloque.Fill(dtBloque);

                comboBox1.DataSource = dtBloque;
                comboBox1.DisplayMember = "nombre_bloque";
                comboBox1.ValueMember = "id_bloque";

                // TIPOS
                SqlDataAdapter daTipo =
                    new SqlDataAdapter(
                        "SELECT id_tipo_ambiente,nombre_tipo FROM TIPO_AMBIENTE",
                        cn);

                DataTable dtTipo = new DataTable();
                daTipo.Fill(dtTipo);

                comboBox2.DataSource = dtTipo;
                comboBox2.DisplayMember = "nombre_tipo";
                comboBox2.ValueMember = "id_tipo_ambiente";

                // ESTADOS
                SqlDataAdapter daEstado =
                    new SqlDataAdapter(
                        "SELECT id_estado_ambiente,nombre_estado FROM ESTADO_AMBIENTE",
                        cn);

                DataTable dtEstado = new DataTable();
                daEstado.Fill(dtEstado);

                comboBox3.DataSource = dtEstado;
                comboBox3.DisplayMember = "nombre_estado";
                comboBox3.ValueMember = "id_estado_ambiente";

                cn.Close();
            }
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DesactivarAmbiente()
        {
            if (selectedId == null)
                return;

            string conn =
                Properties.Settings.Default.SistemaReservasUABConnectionString;

            using (SqlConnection cn = new SqlConnection(conn))
            {
                SqlCommand cmd =
                    new SqlCommand(
                        @"UPDATE AMBIENTE
                  SET id_estado_ambiente = 2
                  WHERE id_ambiente=@id",
                        cn);

                cmd.Parameters.AddWithValue(
                    "@id",
                    selectedId);

                cn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadAmbientes();
        }

        private void btnEditar_Click_1(object sender, EventArgs e)
        {
            EditarAmbiente();
        }

        private void EditarAmbiente()
        {
            if (selectedId == null)
            {
                MessageBox.Show("Seleccione un ambiente");
                return;
            }

            try
            {
                string conn =
                    Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (SqlConnection cn = new SqlConnection(conn))
                {
                    string sql = @"
UPDATE AMBIENTE
SET
    codigo_ambiente=@codigo,
    id_bloque=@bloque,
    id_tipo_ambiente=@tipo,
    id_estado_ambiente=@estado,
    capacidad_maxima=@capacidad
WHERE id_ambiente=@id";

                    SqlCommand cmd =
                        new SqlCommand(sql, cn);

                    cmd.Parameters.AddWithValue("@id", selectedId);

                    cmd.Parameters.AddWithValue(
                        "@codigo",
                        textBox1.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@bloque",
                        comboBox1.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@tipo",
                        comboBox2.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@estado",
                        comboBox3.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@capacidad",
                        numericUpDown1.Value);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Ambiente actualizado");

                LoadAmbientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            VerCaracteristicas();
        }

        private void VerCaracteristicas()
        {
            if (selectedId == null)
            {
                MessageBox.Show(
                    "Seleccione un ambiente.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            try
            {
                string conn =
                    Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (SqlConnection cn = new SqlConnection(conn))
                {
                    string sql = @"
SELECT
    c.descripcion,
    ac.valor
FROM AMBIENTE_CARACTERISTICA ac
INNER JOIN CARACTERISTICA c
    ON ac.id_caracteristica = c.id_caracteristica
WHERE ac.id_ambiente = @id";

                    SqlCommand cmd =
                        new SqlCommand(sql, cn);

                    cmd.Parameters.AddWithValue(
                        "@id",
                        selectedId.Value);

                    cn.Open();

                    SqlDataReader dr =
                        cmd.ExecuteReader();

                    StringBuilder sb =
                        new StringBuilder();

                    while (dr.Read())
                    {
                        sb.AppendLine(
                            dr["descripcion"].ToString()
                            + " : "
                            + dr["valor"].ToString());
                    }

                    cn.Close();

                    if (sb.Length == 0)
                    {
                        MessageBox.Show(
                            "Este ambiente no tiene características registradas.");
                    }
                    else
                    {
                        MessageBox.Show(
                            sb.ToString(),
                            "Características del Ambiente",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

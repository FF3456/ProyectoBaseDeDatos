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
    public partial class FrmEventos : Form
    {
        private DataTable dtEventos;
        private string idColumnName;
        private int? selectedId;

        public FrmEventos()
        {
            InitializeComponent();
            this.Load += FrmEventos_Load;
            this.btnBuscar.Click += btnBuscar_Click;
            this.btnNuevo.Click += btnNuevo_Click;
            this.btnLimpiar.Click += btnLimpiar_Click;
            this.btnGuardar.Click += btnGuardar_Click;
            this.btnEditar.Click += btnEditar_Click;
            this.btnEliminar.Click += btnEliminar_Click;
            this.dgvEventos.CellClick += dgvEventos_CellClick;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FrmEventos_Load(object sender, EventArgs e)
        {
            LoadEventos();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            LoadEventos(txtBuscar.Text.Trim());
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void dgvEventos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var drv = dgvEventos.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) return;
            var row = drv.Row;
            if (dtEventos == null) return;

            if (string.IsNullOrEmpty(idColumnName)) idColumnName = GetIdColumnName(dtEventos);
            object idVal = row[idColumnName];
            int temp;
            if (idVal != null && int.TryParse(idVal.ToString(), out temp)) selectedId = temp; else selectedId = null;

            textBox1.Text = GetRowString(row, new[] { "nombre", "nombre_evento", "nombreevento" });
            textBox2.Text = GetRowString(row, new[] { "responsable", "respons" });
            string asistentes = GetRowString(row, new[] { "asistentes", "cantidad", "cantidad_asistentes" });
            int a; if (int.TryParse(asistentes, out a)) numericUpDown1.Value = a; else numericUpDown1.Value = 0;
            richTextBox1.Text = GetRowString(row, new[] { "requer", "requerimientos", "requerimiento" });
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            SaveEvento(isUpdate: false);
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                MessageBox.Show("Seleccione un evento para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SaveEvento(isUpdate: true);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (selectedId == null)
            {
                MessageBox.Show("Seleccione un evento para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("¿Eliminar el evento seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteEvento();
            }
        }

        private void ClearForm()
        {
            selectedId = null;
            textBox1.Clear();
            textBox2.Clear();
            numericUpDown1.Value = 0;
            richTextBox1.Clear();
        }

        private void LoadEventos(string filtro = null)
        {
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;
            string sql = "SELECT * FROM evento";
            if (!string.IsNullOrEmpty(filtro)) sql += " WHERE nombre LIKE @filtro OR responsable LIKE @filtro";

            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    if (!string.IsNullOrEmpty(filtro)) cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dtEventos = dt;
                    dgvEventos.DataSource = dtEventos;
                    idColumnName = GetIdColumnName(dtEventos);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar eventos: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetIdColumnName(DataTable dt)
        {
            if (dt == null) return null;
            var col = dt.Columns.Cast<DataColumn>().FirstOrDefault(c => c.ColumnName.ToLower().Contains("id"));
            if (col != null) return col.ColumnName;
            return dt.Columns.Count > 0 ? dt.Columns[0].ColumnName : null;
        }

        private string FindColumn(DataTable dt, string[] candidates)
        {
            if (dt == null) return null;
            foreach (var c in dt.Columns.Cast<DataColumn>())
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
            var col = FindColumn(dtEventos, candidates);
            if (col == null) return string.Empty;
            var val = row[col];
            return val == null ? string.Empty : val.ToString();
        }

        private void SaveEvento(bool isUpdate)
        {
            if (dtEventos == null)
            {
                MessageBox.Show("No hay datos de eventos cargados.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;
            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    var cols = dtEventos.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                    string idCol = idColumnName;
                    var writeCols = cols.Where(c => c != idCol).ToList();

                    if (isUpdate && selectedId != null)
                    {
                        var sets = writeCols.Select(c => $"[{c}] = @{c}");
                        cmd.CommandText = $"UPDATE evento SET {string.Join(", ", sets)} WHERE [{idCol}] = @id";
                        cmd.Parameters.AddWithValue("@id", selectedId.Value);
                    }
                    else
                    {
                        cmd.CommandText = $"INSERT INTO evento ({string.Join(", ", writeCols.Select(c => "[" + c + "]"))}) VALUES ({string.Join(", ", writeCols.Select(c => "@" + c))})";
                    }

                    foreach (var c in writeCols)
                    {
                        string lower = c.ToLower();
                        object val = DBNull.Value;
                        if (lower.Contains("nombre")) val = (object)textBox1.Text.Trim() ?? DBNull.Value;
                        else if (lower.Contains("respons")) val = (object)textBox2.Text.Trim() ?? DBNull.Value;
                        else if (lower.Contains("asist")) val = (object)Convert.ToInt32(numericUpDown1.Value);
                        else if (lower.Contains("requer")) val = (object)richTextBox1.Text.Trim() ?? DBNull.Value;
                        else val = DBNull.Value;

                        cmd.Parameters.AddWithValue("@" + c, val ?? DBNull.Value);
                    }

                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    LoadEventos();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar evento: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteEvento()
        {
            if (dtEventos == null || selectedId == null) return;
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;
            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cn;
                    cmd.CommandText = $"DELETE FROM evento WHERE [{idColumnName}] = @id";
                    cmd.Parameters.AddWithValue("@id", selectedId.Value);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    LoadEventos();
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar evento: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

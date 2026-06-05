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
    public partial class FrmDisponibilidad : Form
    {
        private DataTable dtAmbientes;
        private DataTable dtReservas;

        public FrmDisponibilidad()
        {
            InitializeComponent();
            this.Load += FrmDisponibilidad_Load;
            this.btnBuscarDisponibilidad.Click += btnBuscarDisponibilidad_Click;
        }

        private void FrmDisponibilidad_Load(object sender, EventArgs e)
        {
            // populate horarios from detalle_horario
            LoadHorarios();
            // initial load
            LoadDisponibilidad();
        }

        private void LoadHorarios()
        {
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;
            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT horario FROM detalle_horario ORDER BY horario", cn))
                {
                    cn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        cbHorario.Items.Clear();
                        while (rdr.Read())
                        {
                            cbHorario.Items.Add(rdr[0].ToString());
                        }
                    }
                    cn.Close();
                }
            }
            catch
            {
                // ignore - leave designer items if table not present
            }
        }

        private void btnBuscarDisponibilidad_Click(object sender, EventArgs e)
        {
            LoadDisponibilidad();
        }

        public void LoadDisponibilidad()
        {
            string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;

            DateTime fecha = dateTimePicker1.Value.Date;
            string horario = cbHorario.Text;
            int capacidadMin = (int)numCapacidad.Value;
            string tipo = cbTipoAmbiente.Text;
            string caracteristicas = clbCaracteristicas.Text; // combo box with comma-separated or single selection

            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                {
                    cn.Open();

                    // load ambientes
                    using (SqlCommand cmdAmb = new SqlCommand("SELECT * FROM ambiente", cn))
                    using (SqlDataAdapter daAmb = new SqlDataAdapter(cmdAmb))
                    {
                        dtAmbientes = new DataTable();
                        daAmb.Fill(dtAmbientes);
                    }

                    // Try to load reservas from common tables. First try 'reserva' with columns 'fecha' and 'horario'
                    dtReservas = null;
                    string reservaTable = "reserva";
                    string fechaCol = "fecha";
                    string horarioCol = "horario";

                    bool loaded = false;

                    // candidate: direct table 'reserva'
                    try
                    {
                        using (SqlCommand cmdRes = new SqlCommand($"SELECT * FROM [{reservaTable}] WHERE [{fechaCol}] = @fecha AND [{horarioCol}] = @horario", cn))
                        {
                            cmdRes.Parameters.AddWithValue("@fecha", fecha);
                            cmdRes.Parameters.AddWithValue("@horario", horario);
                            using (SqlDataAdapter daRes = new SqlDataAdapter(cmdRes))
                            {
                                dtReservas = new DataTable();
                                daRes.Fill(dtReservas);
                                loaded = true; // query executed (may be empty)
                            }
                        }
                    }
                    catch { loaded = false; dtReservas = null; }

                    // If failed or dtReservas is empty, try to detect a table that contains a date-like column and a horario-like column
                    if (!loaded || dtReservas == null)
                    {
                        // candidate column name patterns
                        string[] dateCandidates = new[] { "fecha", "fecha_reserva", "fecha_reserv", "fecha_inicio", "f_reserva" };
                        string[] horarioCandidates = new[] { "horario", "id_horario", "hora" };

                        // find a table that has at least one date column and one horario column
                        using (var cmd = new SqlCommand())
                        {
                            cmd.Connection = cn;
                            cmd.CommandText = @"SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE LOWER(COLUMN_NAME) LIKE '%' + @pat + '%'";
                            // we'll query INFORMATION_SCHEMA multiple times below
                        }

                        // find tables containing dateCandidate and horarioCandidate
                        string foundTable = null;
                        string foundDateCol = null;
                        string foundHorarioCol = null;

                        // get list of tables
                        var tables = new List<string>();
                        using (var cmdTables = new SqlCommand("SELECT DISTINCT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS", cn))
                        using (var rdr = cmdTables.ExecuteReader())
                        {
                            while (rdr.Read()) tables.Add(rdr.GetString(0));
                        }

                        foreach (var tbl in tables)
                        {
                            // get columns for this table
                            var cols = new List<string>();
                            using (var cmdCols = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t", cn))
                            {
                                cmdCols.Parameters.AddWithValue("@t", tbl);
                                using (var rdr = cmdCols.ExecuteReader())
                                {
                                    while (rdr.Read()) cols.Add(rdr.GetString(0).ToLower());
                                }
                            }

                            string dcol = cols.FirstOrDefault(c => dateCandidates.Any(dc => c.Contains(dc)));
                            string hcol = cols.FirstOrDefault(c => horarioCandidates.Any(hc => c.Contains(hc)));
                            if (dcol != null && hcol != null)
                            {
                                foundTable = tbl;
                                foundDateCol = dcol;
                                foundHorarioCol = hcol;
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(foundTable))
                        {
                            try
                            {
                                using (SqlCommand cmdRes = new SqlCommand($"SELECT * FROM [{foundTable}] WHERE [{foundDateCol}] = @fecha AND [{foundHorarioCol}] = @horario", cn))
                                {
                                    cmdRes.Parameters.AddWithValue("@fecha", fecha);
                                    cmdRes.Parameters.AddWithValue("@horario", horario);
                                    using (SqlDataAdapter daRes = new SqlDataAdapter(cmdRes))
                                    {
                                        dtReservas = new DataTable();
                                        daRes.Fill(dtReservas);
                                        reservaTable = foundTable;
                                        fechaCol = foundDateCol;
                                        horarioCol = foundHorarioCol;
                                    }
                                }
                            }
                            catch { dtReservas = null; }
                        }
                    }

                    cn.Close();
                }

                // Decide column names
                string ambIdCol = GetIdColumnName(dtAmbientes);
                string resAmbCol = FindColumn(dtReservas, new[] { "idamb", "ambiente", "ambiente_id", "id_ambiente", "idambiente" });

                // build result table (clone structure or create custom)
                DataTable result = dtAmbientes.Clone();

                foreach (DataRow amb in dtAmbientes.Rows)
                {
                    // filter by estado (prefer 'Disponible') if column exists
                    var estadoCol = FindColumn(dtAmbientes, new[] { "estado", "status" });
                    if (estadoCol != null)
                    {
                        var est = amb[estadoCol]?.ToString();
                        if (!string.IsNullOrEmpty(est) && (est.ToLower().Contains("inhabil") || est.ToLower().Contains("manten")))
                        {
                            continue; // skip non-available states
                        }
                    }

                    // capacidad filter
                    var capCol = FindColumn(dtAmbientes, new[] { "capacidad", "cap" });
                    if (capCol != null && capacidadMin > 0)
                    {
                        int cap = 0;
                        int.TryParse(amb[capCol]?.ToString(), out cap);
                        if (cap < capacidadMin) continue;
                    }

                    // tipo filter
                    var tipoCol = FindColumn(dtAmbientes, new[] { "tipo", "tipo_ambiente" });
                    if (!string.IsNullOrEmpty(tipo) && tipoCol != null)
                    {
                        var t = amb[tipoCol]?.ToString();
                        if (!string.IsNullOrEmpty(t) && !t.ToLower().Contains(tipo.ToLower())) continue;
                    }

                    // caracteristicas filter (comma separated expected)
                    var carCol = FindColumn(dtAmbientes, new[] { "caracter", "caract", "features" });
                    if (!string.IsNullOrEmpty(caracteristicas) && carCol != null)
                    {
                        var carVal = amb[carCol]?.ToString() ?? string.Empty;
                        var want = caracteristicas.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToLower());
                        bool ok = true;
                        foreach (var w in want)
                        {
                            if (!carVal.ToLower().Contains(w)) { ok = false; break; }
                        }
                        if (!ok) continue;
                    }

                    // check reservas: if this ambiente is reserved for the date+horario skip
                    bool ocupado = false;
                    if (dtReservas != null && dtReservas.Rows.Count > 0 && resAmbCol != null)
                    {
                        var ambId = amb[ambIdCol]?.ToString();
                        foreach (DataRow r in dtReservas.Rows)
                        {
                            var rid = r[resAmbCol]?.ToString();
                            if (string.IsNullOrEmpty(ambId) || string.IsNullOrEmpty(rid)) continue;
                            if (ambId == rid) { ocupado = true; break; }
                        }
                    }

                    if (!ocupado)
                    {
                        result.ImportRow(amb);
                    }
                }

                dgvDisponibilidad.DataSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar disponibilidad: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}

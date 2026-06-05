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
    public partial class FrmReservas : Form
    {
        private DataTable dtAmbientes;

        public FrmReservas()
        {
            InitializeComponent();
            this.Load += FrmReservas_Load;
            this.button1.Click += buttonBuscarAmbientes_Click;
            this.button2.Click += buttonReservar_Click;
            this.button3.Click += buttonLimpiar_Click;
            this.button5.Click += buttonVerDetalle_Click;
            this.btnBuscarMisReservas.Click += BtnBuscarMisReservas_Click;
            this.btnEliminarReserva.Click += BtnEliminarReserva_Click;

            checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
        }

        private void FrmReservas_Load(object sender, EventArgs e)
        {
            LoadUsuariosIntoCombo();

            comboBox3.Items.Clear();

            comboBox3.Items.Add("Diaria");
            comboBox3.Items.Add("Semanal");
            comboBox3.Items.Add("Mensual");
            comboBox3.Items.Add("Anual");

            comboBox3.SelectedIndex = 1;

            comboBox3.Visible = false;
            dateTimePicker4.Visible = false;
            dateTimePicker5.Visible = false;

            dateTimePicker2.Format = DateTimePickerFormat.Time;
            dateTimePicker2.ShowUpDown = true;

            dateTimePicker3.Format = DateTimePickerFormat.Time;
            dateTimePicker3.ShowUpDown = true;

            comboBox3.Visible = false;
dateTimePicker4.Visible = false;
dateTimePicker5.Visible = false;

label11.Visible = false;
label12.Visible = false;
label13.Visible = false;

        }

        private void buttonBuscarAmbientes_Click(object sender, EventArgs e)
        {
            BuscarAmbientesDisponibles();
        }

        private void buttonReservar_Click(object sender, EventArgs e)
        {
            ReservarSeleccionado();
        }

        private void buttonLimpiar_Click(object sender, EventArgs e)
        {
            ClearForm();
        }


        private void buttonVerDetalle_Click(object sender, EventArgs e)
        {
            VerDetalleAmbiente();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool mostrar = checkBox1.Checked;

            comboBox3.Visible = mostrar;
            dateTimePicker4.Visible = mostrar;
            dateTimePicker5.Visible = mostrar;

            label11.Visible = mostrar;
            label12.Visible = mostrar;
            label13.Visible = mostrar;
        }

        private void LoadUsuariosIntoCombo()
        {
            try
            {
                string conn = Properties.Settings.Default.SistemaReservasUABConnectionString;
                using (var cn = new SqlConnection(conn))
                using (var cmd = new SqlCommand("SELECT username FROM usuario", cn))
                {
                    cn.Open();
                    using (var rdr = cmd.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (rdr.Read()) comboBox1.Items.Add(rdr[0].ToString());
                    }
                    cn.Close();
                }
            }
            catch { }
        }

        private int ObtenerIdUsuario(string username)
        {
            string conn = Properties.Settings.Default.SistemaReservasUABConnectionString;

            using (SqlConnection cn = new SqlConnection(conn))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT id_usuario FROM USUARIO WHERE username=@u", cn))
            {
                cmd.Parameters.AddWithValue("@u", username);

                cn.Open();

                object result = cmd.ExecuteScalar();

                if (result == null)
                    return 0;

                return Convert.ToInt32(result);
            }
        }

        private void BuscarAmbientesDisponibles()
        {
            try
            {
                string conn =
                    Properties.Settings.Default.SistemaReservasUABConnectionString;

                using (SqlConnection cn = new SqlConnection(conn))
                {
                    DateTime fecha = dateTimePicker1.Value.Date;

                    TimeSpan horaInicio =
                        dateTimePicker2.Value.TimeOfDay;

                    TimeSpan horaFin =
                        dateTimePicker3.Value.TimeOfDay;

                    int capacidad =
                        (int)numericUpDown1.Value;

                    string sql = @"
SELECT
    a.id_ambiente,
    a.codigo_ambiente,
    a.capacidad_maxima,
    b.nombre_bloque,
    ta.nombre_tipo

FROM AMBIENTE a

INNER JOIN BLOQUE b
    ON a.id_bloque = b.id_bloque

INNER JOIN TIPO_AMBIENTE ta
    ON a.id_tipo_ambiente = ta.id_tipo_ambiente

WHERE
    a.capacidad_maxima >= @capacidad

AND a.id_ambiente NOT IN
(
    SELECT r.id_ambiente

    FROM RESERVA r

    INNER JOIN DETALLE_HORARIO dh
        ON r.id_reserva = dh.id_reserva

    WHERE
        dh.fecha_especifica = @fecha
        AND dh.hora_inicio < @horaFin
        AND dh.hora_fin > @horaInicio
)

ORDER BY a.codigo_ambiente";

                    SqlDataAdapter da =
                        new SqlDataAdapter(sql, cn);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@fecha", fecha);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@horaInicio", horaInicio);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@horaFin", horaFin);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@capacidad", capacidad);

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    dgvAmbientesDisponibles.DataSource = dt;
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
            if (dgvAmbientesDisponibles.Columns.Contains("id_ambiente"))
            {
                dgvAmbientesDisponibles.Columns["id_ambiente"].Visible = false;
            }
        }

        private void ReservarSeleccionado()
        {
            if (dgvAmbientesDisponibles.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un ambiente.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Seleccione un usuario.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DateTime fecha = dateTimePicker1.Value.Date;

            TimeSpan horaInicio =
                dateTimePicker2.Value.TimeOfDay;

            TimeSpan horaFin =
                dateTimePicker3.Value.TimeOfDay;

            if (horaInicio >= horaFin)
            {
                MessageBox.Show(
                    "La hora inicio debe ser menor a la hora fin.");

                return;
            }

            int idAmbiente =
                Convert.ToInt32(
                    dgvAmbientesDisponibles
                    .CurrentRow
                    .Cells["id_ambiente"]
                    .Value);

            int idUsuario =
                ObtenerIdUsuario(comboBox1.Text);

            string motivo = comboBox2.Text;

            int asistentes =
                (int)numericUpDown1.Value;

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            try
            {
                using (SqlConnection cn =
                       new SqlConnection(conn))
                {
                    cn.Open();

                    SqlTransaction tr =
                        cn.BeginTransaction();

                    try
                    {
                        int idReserva;

                        string sqlReserva = @"
INSERT INTO RESERVA
(
    id_usuario_solicitante,
    id_ambiente,
    id_estado_reserva,
    motivo,
    cantidad_asistentes
)
VALUES
(
    @usuario,
    @ambiente,
    1,
    @motivo,
    @cantidad
);

SELECT CAST(SCOPE_IDENTITY() AS INT);
";

                        using (SqlCommand cmd =
                               new SqlCommand(
                                   sqlReserva,
                                   cn,
                                   tr))
                        {
                            cmd.Parameters.AddWithValue(
                                "@usuario",
                                idUsuario);

                            cmd.Parameters.AddWithValue(
                                "@ambiente",
                                idAmbiente);

                            cmd.Parameters.AddWithValue(
                                "@motivo",
                                motivo);

                            cmd.Parameters.AddWithValue(
                                "@cantidad",
                                asistentes);

                            idReserva =
                                Convert.ToInt32(
                                    cmd.ExecuteScalar());
                        }

                        string sqlHorario = @"
INSERT INTO DETALLE_HORARIO
(
    id_reserva,
    fecha_especifica,
    hora_inicio,
    hora_fin
)
VALUES
(
    @idReserva,
    @fecha,
    @horaInicio,
    @horaFin
)";

                        using (SqlCommand cmd =
                               new SqlCommand(
                                   sqlHorario,
                                   cn,
                                   tr))
                        {
                            cmd.Parameters.AddWithValue(
                                "@idReserva",
                                idReserva);

                            cmd.Parameters.AddWithValue(
                                "@fecha",
                                fecha);

                            cmd.Parameters.AddWithValue(
                                "@horaInicio",
                                horaInicio);

                            cmd.Parameters.AddWithValue(
                                "@horaFin",
                                horaFin);

                            cmd.ExecuteNonQuery();
                        }

                        // RESERVA RECURRENTE
                        if (checkBox1.Checked)
                        {
                            string sqlRec = @"
INSERT INTO RECURRENCIA
(
    id_reserva,
    id_tipo_frecuencia,
    fecha_inicio,
    fecha_fin
)
VALUES
(
    @idReserva,
    @frecuencia,
    @inicio,
    @fin
)";

                            using (SqlCommand cmd =
                                   new SqlCommand(
                                       sqlRec,
                                       cn,
                                       tr))
                            {
                                cmd.Parameters.AddWithValue(
                                    "@idReserva",
                                    idReserva);

                                cmd.Parameters.AddWithValue(
                                    "@frecuencia",
                                    comboBox3.SelectedIndex + 1);

                                cmd.Parameters.AddWithValue(
                                    "@inicio",
                                    dateTimePicker4.Value.Date);

                                cmd.Parameters.AddWithValue(
                                    "@fin",
                                    dateTimePicker5.Value.Date);

                                cmd.ExecuteNonQuery();
                            }
                        }

                        tr.Commit();

                        MessageBox.Show(
                            "Reserva registrada correctamente.",
                            "Éxito",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        BuscarAmbientesDisponibles();
                    }
                    catch
                    {
                        tr.Rollback();
                        throw;
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


        private void VerDetalleAmbiente()
        {
            if (dgvAmbientesDisponibles.CurrentRow == null) return;
            var drv = dgvAmbientesDisponibles.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;
            var row = drv.Row;
            var sb = new StringBuilder();
            foreach (DataColumn c in row.Table.Columns)
            {
                sb.AppendLine($"{c.ColumnName}: {row[c]}");
            }
            MessageBox.Show(sb.ToString(), "Detalle Ambiente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearForm()
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;

            numericUpDown1.Value = 0;

            dateTimePicker1.Value = DateTime.Today;

            dateTimePicker2.Value =
                DateTime.Today.AddHours(8);

            dateTimePicker3.Value =
                DateTime.Today.AddHours(10);

            checkBox1.Checked = false;

            comboBox3.SelectedIndex = 0;

            dateTimePicker4.Value = DateTime.Today;
            dateTimePicker5.Value = DateTime.Today;

            for (int i = 0; i < clbRequerimientos.Items.Count; i++)
            {
                clbRequerimientos.SetItemChecked(i, false);
            }

            dgvAmbientesDisponibles.DataSource = null;
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
                foreach (var cand in candidates) if (name.Contains(cand.ToLower())) return c.ColumnName;
            }
            return null;
        }

        private string GetIdColumnName(DataRow row)
        {
            return GetIdColumnName(row.Table);
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void CargarReservasUsuario()
        {
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Seleccione un usuario.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int idUsuario =
                ObtenerIdUsuario(comboBox1.Text);

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            try
            {
                using (SqlConnection cn =
                       new SqlConnection(conn))
                {
                    string sql = @"
SELECT
    r.id_reserva,
    a.codigo_ambiente,
    r.motivo,
    r.cantidad_asistentes,
    dh.fecha_especifica,
    dh.hora_inicio,
    dh.hora_fin,
    er.nombre_estado
FROM RESERVA r

INNER JOIN AMBIENTE a
    ON r.id_ambiente = a.id_ambiente

INNER JOIN DETALLE_HORARIO dh
    ON r.id_reserva = dh.id_reserva

INNER JOIN ESTADO_RESERVA er
    ON r.id_estado_reserva = er.id_estado_reserva

WHERE r.id_usuario_solicitante = @usuario
AND r.id_estado_reserva <> 2

ORDER BY dh.fecha_especifica DESC";

                    SqlDataAdapter da =
                        new SqlDataAdapter(sql, cn);

                    da.SelectCommand.Parameters.AddWithValue(
                        "@usuario",
                        idUsuario);

                    DataTable dt =
                        new DataTable();

                    da.Fill(dt);

                    dgvReservas.DataSource = dt;

                    if (dgvReservas.Columns.Contains("id_reserva"))
                    {
                        dgvReservas.Columns["id_reserva"].Visible = false;
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

        private void CancelarReserva()
        {
            if (dgvReservas.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione una reserva.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            DialogResult r =
                MessageBox.Show(
                    "¿Desea cancelar esta reserva?",
                    "Confirmación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (r != DialogResult.Yes)
                return;

            int idReserva =
                Convert.ToInt32(
                    dgvReservas
                    .CurrentRow
                    .Cells["id_reserva"]
                    .Value);

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            try
            {
                using (SqlConnection cn =
                       new SqlConnection(conn))
                {
                    cn.Open();

                    string sql = @"
UPDATE RESERVA
SET
    id_estado_reserva = 2,
    motivo_cancelacion = 'Cancelada por el usuario'
WHERE id_reserva = @id";

                    using (SqlCommand cmd =
                           new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idReserva);

                        cmd.ExecuteNonQuery();
                    }
                }

                CargarReservasUsuario();
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
        private void BtnEliminarReserva_Click(
    object sender,
    EventArgs e)
        {
            CancelarReserva();
        }

        private void BtnBuscarMisReservas_Click(object sender, EventArgs e)
        {
            CargarReservasUsuario();
        }

        private void FrmReservas_Load_1(object sender, EventArgs e)
        {

        }
    }
}

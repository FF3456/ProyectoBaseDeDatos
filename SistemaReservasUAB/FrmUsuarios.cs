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
    public partial class FrmUsuarios : Form
    {
        public FrmUsuarios()
        {
            InitializeComponent();

            this.Load += FrmUsuarios_Load;

            this.btnBuscar.Click += btnBuscar_Click;

            btnNuevo.Click += btnNuevo_Click;
            btnGuardar.Click += button2_Click;
            btnEditar.Click += btnEditar_Click;
            btnDesactivar.Click += button1_Click;
            btnReactivar.Click += btnReactivar_Click;

            this.cmbTipoUsuario.SelectedIndexChanged +=
                cmbTipoUsuario_SelectedIndexChanged;
        }

        private void cmbTipoUsuario_SelectedIndexChanged(
    object sender,
    EventArgs e)
        {
            GenerarCodigoUsuario();
            MostrarCamposPorTipo();
        }

        private void GenerarCodigoUsuario()
        {
            if (cmbTipoUsuario.SelectedIndex < 0)
                return;

            string prefijo = "";

            switch (cmbTipoUsuario.Text)
            {
                case "Estudiante":
                    prefijo = "EST";
                    break;

                case "Docente":
                    prefijo = "DOC";
                    break;

                case "Administrador":
                    prefijo = "ADM";
                    break;
            }

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
SELECT TOP 1 codigo_usuario
FROM USUARIO
WHERE codigo_usuario LIKE @prefijo + '%'
ORDER BY codigo_usuario DESC";

                    using (SqlCommand cmd =
                           new SqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@prefijo",
                            prefijo);

                        object resultado =
                            cmd.ExecuteScalar();

                        int siguienteNumero = 1;

                        if (resultado != null)
                        {
                            string ultimoCodigo =
                                resultado.ToString();

                            string numero =
                                ultimoCodigo.Substring(3);

                            siguienteNumero =
                                Convert.ToInt32(numero) + 1;
                        }

                        txtCodigoUsuario.Text =
                            prefijo +
                            siguienteNumero.ToString("000");
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

        private void MostrarCamposPorTipo()
        {
            label10.Visible = false;
            txtCarrera.Visible = false;

            lblAreaDepartamento.Visible = false;
            txtAreaDepartamento.Visible = false;

            label12.Visible = false;
            txtNivelPermiso.Visible = false;

            switch (cmbTipoUsuario.Text)
            {
                case "Estudiante":

                    label10.Visible = true;
                    txtCarrera.Visible = true;

                    break;

                case "Docente":

                    lblAreaDepartamento.Visible = true;
                    txtAreaDepartamento.Visible = true;

                    break;

                case "Administrador":

                    label12.Visible = true;
                    txtNivelPermiso.Visible = true;

                    break;
            }
        }
        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void FrmUsuarios_Load(object sender, EventArgs e)
        {
            LoadUsuarios();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            LoadUsuarios(filtro);
        }

        private void LoadUsuarios(string filtro = null)
        {
            string connStr =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            string sql = @"
SELECT
id_usuario,
codigo_usuario,
nombre_completo,
correo,
telefono,
username,
estado
FROM USUARIO";

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                sql += @"
WHERE
nombre_completo LIKE @filtro
OR username LIKE @filtro
OR correo LIKE @filtro
OR codigo_usuario LIKE @filtro";
            }

            try
            {
                using (SqlConnection cn =
                       new SqlConnection(connStr))
                using (SqlCommand cmd =
                       new SqlCommand(sql, cn))
                using (SqlDataAdapter da =
                       new SqlDataAdapter(cmd))
                {
                    if (!string.IsNullOrWhiteSpace(filtro))
                    {
                        cmd.Parameters.AddWithValue(
                            "@filtro",
                            "%" + filtro + "%");
                    }

                    DataTable dt =
                        new DataTable();

                    da.Fill(dt);

                    dgvUsuarios.DataSource = dt;

                    if (dgvUsuarios.Columns.Contains("id_usuario"))
                    {
                        dgvUsuarios.Columns["id_usuario"].Visible = false;
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void DesactivarUsuario()
        {
            if (dgvUsuarios.CurrentRow == null)
                return;

            int id =
                Convert.ToInt32(
                    dgvUsuarios.CurrentRow
                    .Cells["id_usuario"]
                    .Value);

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            using (SqlConnection cn =
                   new SqlConnection(conn))
            {
                cn.Open();

                string sql = @"
UPDATE USUARIO
SET estado = 0
WHERE id_usuario = @id";

                using (SqlCommand cmd =
                       new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue(
                        "@id",
                        id);

                    cmd.ExecuteNonQuery();
                }
            }

            LoadUsuarios();
        }

        private void CambiarEstadoUsuario(
    bool activo)
        {
            if (dgvUsuarios.CurrentRow == null)
                return;

            int id =
                Convert.ToInt32(
                    dgvUsuarios.CurrentRow
                    .Cells["id_usuario"]
                    .Value);

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            using (SqlConnection cn =
                   new SqlConnection(conn))
            {
                cn.Open();

                string sql = @"
UPDATE USUARIO
SET estado=@estado
WHERE id_usuario=@id";

                using (SqlCommand cmd =
                       new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue(
                        "@estado",
                        activo);

                    cmd.Parameters.AddWithValue(
                        "@id",
                        id);

                    cmd.ExecuteNonQuery();
                }
            }

            LoadUsuarios();
        }

        private void btnReactivar_Click(
    object sender,
    EventArgs e)
        {
            CambiarEstadoUsuario(true);
        }

        private void dgvUsuarios_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LimpiarFormulario()
        {
            txtCodigoUsuario.Clear();
            txtNombre.Clear();
            txtCorreo.Clear();
            txtTelefono.Clear();
            txtUsername.Clear();
            txtPassword.Clear();

            txtCarrera.Clear();
            txtAreaDepartamento.Clear();
            txtNivelPermiso.Clear();

            cmbTipoUsuario.SelectedIndex = -1;

            chkActivo.Checked = true;

            dgvUsuarios.ClearSelection();
        }

        private void btnNuevo_Click(
    object sender,
    EventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnGuardar_Click(
    object sender,
    EventArgs e)
        {
            EliminarUsuario();
        }

        private void EliminarUsuario()
        {
            if (dgvUsuarios.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un usuario.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            int idUsuario =
                Convert.ToInt32(
                    dgvUsuarios.CurrentRow
                    .Cells["id_usuario"]
                    .Value);

            DialogResult r =
                MessageBox.Show(
                    "¿Desea eliminar permanentemente este usuario?",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (r != DialogResult.Yes)
                return;

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            try
            {
                using (SqlConnection cn =
                       new SqlConnection(conn))
                {
                    cn.Open();

                    // Verificar reservas asociadas
                    string sqlVerificar = @"
SELECT COUNT(*)
FROM RESERVA
WHERE id_usuario_solicitante = @id";

                    using (SqlCommand cmd =
                           new SqlCommand(sqlVerificar, cn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        int cantidad =
                            Convert.ToInt32(
                                cmd.ExecuteScalar());

                        if (cantidad > 0)
                        {
                            MessageBox.Show(
                                "No se puede eliminar porque el usuario tiene reservas registradas.",
                                "Operación no permitida",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                            return;
                        }
                    }

                    string sqlEliminar =
                        "DELETE FROM USUARIO WHERE id_usuario = @id";

                    using (SqlCommand cmd =
                           new SqlCommand(sqlEliminar, cn))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        cmd.ExecuteNonQuery();
                    }
                }

                
                LoadUsuarios();
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

        private void GuardarUsuario()
        {
            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            SqlConnection cn =
                new SqlConnection(conn);

            cn.Open();

            SqlTransaction tr =
                cn.BeginTransaction();

            try
            {
                string sqlUsuario = @"
INSERT INTO USUARIO
(
    codigo_usuario,
    nombre_completo,
    correo,
    telefono,
    username,
    password_hash,
    estado
)
VALUES
(
    @codigo,
    @nombre,
    @correo,
    @telefono,
    @username,
    @password,
    @estado
);

SELECT SCOPE_IDENTITY();
";

                int idUsuario;

                using (SqlCommand cmd =
                       new SqlCommand(
                           sqlUsuario,
                           cn,
                           tr))
                {
                    cmd.Parameters.AddWithValue(
                        "@codigo",
                        txtCodigoUsuario.Text);

                    cmd.Parameters.AddWithValue(
                        "@nombre",
                        txtNombre.Text);

                    cmd.Parameters.AddWithValue(
                        "@correo",
                        txtCorreo.Text);

                    cmd.Parameters.AddWithValue(
                        "@telefono",
                        txtTelefono.Text);

                    cmd.Parameters.AddWithValue(
                        "@username",
                        txtUsername.Text);

                    cmd.Parameters.AddWithValue(
                        "@password",
                        txtPassword.Text);

                    cmd.Parameters.AddWithValue(
                        "@estado",
                        chkActivo.Checked);

                    idUsuario =
                        Convert.ToInt32(
                            cmd.ExecuteScalar());
                }

                if (cmbTipoUsuario.Text == "Estudiante")
                {
                    string sql = @"
INSERT INTO ESTUDIANTE
(
    id_usuario,
    carrera
)
VALUES
(
    @id,
    @carrera
)";
                    using (SqlCommand cmd =
                           new SqlCommand(
                               sql,
                               cn,
                               tr))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        cmd.Parameters.AddWithValue(
                            "@carrera",
                            txtCarrera.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                if (cmbTipoUsuario.Text == "Docente")
                {
                    string sql = @"
INSERT INTO DOCENTE
(
    id_usuario,
    area_departamento
)
VALUES
(
    @id,
    @area
)";
                    using (SqlCommand cmd =
                           new SqlCommand(
                               sql,
                               cn,
                               tr))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        cmd.Parameters.AddWithValue(
                            "@area",
                            txtAreaDepartamento.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                if (cmbTipoUsuario.Text == "Administrador")
                {
                    string sql = @"
INSERT INTO ADMINISTRADOR
(
    id_usuario,
    nivel_permiso
)
VALUES
(
    @id,
    @nivel
)";
                    using (SqlCommand cmd =
                           new SqlCommand(
                               sql,
                               cn,
                               tr))
                    {
                        cmd.Parameters.AddWithValue(
                            "@id",
                            idUsuario);

                        cmd.Parameters.AddWithValue(
                            "@nivel",
                            txtNivelPermiso.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

                tr.Commit();

                MessageBox.Show(
                    "Usuario guardado correctamente");

                LoadUsuarios();
            }
            catch (Exception ex)
            {
                tr.Rollback();

                MessageBox.Show(ex.Message);
            }

            cn.Close();
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            CambiarEstadoUsuario(true);
        }

        private void btnEditar_Click(
    object sender,
    EventArgs e)
        {
            EditarUsuario();
        }

        private void EditarUsuario()
        {
            if (dgvUsuarios.CurrentRow == null)
                return;

            int id =
                Convert.ToInt32(
                    dgvUsuarios.CurrentRow
                    .Cells["id_usuario"]
                    .Value);

            string conn =
                Properties.Settings.Default
                .SistemaReservasUABConnectionString;

            using (SqlConnection cn =
                   new SqlConnection(conn))
            {
                cn.Open();

                string sql = @"
UPDATE USUARIO
SET
nombre_completo=@nombre,
correo=@correo,
telefono=@telefono,
username=@user,
estado=@estado
WHERE id_usuario=@id";

                using (SqlCommand cmd =
                       new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue(
                        "@nombre",
                        txtNombre.Text);

                    cmd.Parameters.AddWithValue(
                        "@correo",
                        txtCorreo.Text);

                    cmd.Parameters.AddWithValue(
                        "@telefono",
                        txtTelefono.Text);

                    cmd.Parameters.AddWithValue(
                        "@user",
                        txtUsername.Text);

                    cmd.Parameters.AddWithValue(
                        "@estado",
                        chkActivo.Checked);

                    cmd.Parameters.AddWithValue(
                        "@id",
                        id);

                    cmd.ExecuteNonQuery();
                }
            }

            LoadUsuarios();
        }

        private void button1_Click(object sender,
    EventArgs e)
        {
                CambiarEstadoUsuario(false);
         }

        private void button2_Click(object sender, EventArgs e)
        {
            GuardarUsuario();
        }
    }
}

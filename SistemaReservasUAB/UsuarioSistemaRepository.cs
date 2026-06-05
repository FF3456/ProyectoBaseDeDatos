using System;
using System;
using System.Data.SqlClient;

namespace SistemaReservasUAB
{
    public class UsuarioSistemaRepository
    {
        private readonly string connStr = Properties.Settings.Default.SistemaReservasUABConnectionString;

        public UsuarioSistema Login(string username, string password)
        {
            string sql = @"
SELECT Usuario AS usuario, Rol AS rol
FROM UsuarioSistema
WHERE Usuario = @username
AND PasswordHash = @password
AND Estado = 1";

            try
            {
                using (SqlConnection cn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    cn.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            return new UsuarioSistema
                            {
                                Usuario = rdr["usuario"].ToString(),
                                Rol = rdr["rol"].ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore and return null for invalid login or DB errors.
            }

            return null;
        }
    }
}

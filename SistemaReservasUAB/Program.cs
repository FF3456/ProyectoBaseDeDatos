using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SistemaReservasUAB
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Mostrar primero el formulario de login
            using (var login = new FrmLogin())
            {
                var result = login.ShowDialog();

                if (result == DialogResult.OK && login.AuthenticatedUser != null)
                {
                    var main = new Form1();
                    main.UsuarioActual = login.AuthenticatedUser.Usuario;
                    main.RolActual = login.AuthenticatedUser.Rol;
                    main.ActualizarUsuario();
                    Application.Run(main);
                }
                else
                {
                    // Si el usuario cancela o no se autentica, salir
                    return;
                }
            }
        }
    }
}

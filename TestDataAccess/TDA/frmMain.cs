using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Data;
using System.Windows.Forms;

namespace TDA
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            Result resultado = DataAccess<Object>.SelectOther("productos-recetas", "sp_productos-recetas_selectall");
            DataTable dt = resultado.Data;
            //Result resultado = DataAccess<Object>.SelectOther("usuarios", "sp_usuarios_selectall", new Parameter("_id", null),
            //new Parameter("_nombre", "Mochila"),
            //new Parameter("_cantidadTotal", null);
        }
    }
}

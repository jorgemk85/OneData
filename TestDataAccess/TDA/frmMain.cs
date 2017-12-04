using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Result resultado = DataAccess<Object>.SelectOther("usuarios", "sp_usuarios_selectall");
            DataTable dt = resultado.Data;
            //Result resultado = DataAccess<Object>.SelectOther("usuarios", "sp_usuarios_selectall", new Parameter("_id", null),
            //new Parameter("_nombre", "Mochila"),
            //new Parameter("_cantidadTotal", null);
        }
    }
}

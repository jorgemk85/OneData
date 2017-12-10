using DataAccess.BO;
using DataAccess.DAO;
using System;
using System.Collections.Generic;
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
           // DataTable dt = DataAccess<Object>.SelectOther("productos", "sp_productos_selectall").Data;

            List<Log> list = DataAccess<Log>.SelectAllList();
        }
    }
}

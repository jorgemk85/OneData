using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace TDA
{
    public partial class frmMain : Form
    {
        Connector connector = new Connector();
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            List<Log> list = connector.SelectAllList<Log>();
            Debug.Write("Hola");
        }
    }
}

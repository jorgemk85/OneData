using DataAccess.BO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
            RunAsync();
            Debug.Write("Hola");
        }

        private async void RunAsync()
        {
            Task<List<Log>> task = new Task<List<Log>>(() => connector.SelectAllList<Log>());
            task.Start();
            await task;
        }
    }
}

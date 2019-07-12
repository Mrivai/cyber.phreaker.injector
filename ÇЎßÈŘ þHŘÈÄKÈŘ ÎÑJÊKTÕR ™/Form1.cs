using System;
using MetroFramework.Forms;
using ÇЎßÈŘ_þHŘÈÄKÈŘ_ÎÑJÊKTÕR__.Simple_Server;

namespace ÇЎßÈŘ_þHŘÈÄKÈŘ_ÎÑJÊKTÕR__
{
    public partial class Form1 : MetroForm
    {
        private HTTP_Proxy Hi;
        public Form1()
        {
            InitializeComponent();
            // untuk menjalankan hi Hi.start()
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void simpanSS_Click(object sender, EventArgs e)
        {
            
                   
        }


       

        private void Menubtn_Click(object sender, EventArgs e)
        {
            this.MainPage.Visible = false;
            this.Rootpnl.Visible = true;
        }

        private void BackHome_Click(object sender, EventArgs e)
        {
            this.MainPage.Visible = true;
            this.Rootpnl.Visible =false;
        }

        private void onof_Click(object sender, EventArgs e)
        {
            string LPORT = "8181";
            string RPROXY = rproxy.Text;
            string RPORT = rport.Text;
            string METHOD = CBmethod.SelectedItem.ToString();
            string URL = injekurl.Text;
            string HOST = injekhost.Text;
            Hi = new HTTP_Proxy(LPORT, RPROXY, RPORT, METHOD, URL, HOST);
            Hi.Star();
        }

        private void metroButton12_Click(object sender, EventArgs e)
        {
            this.PnlSSHMaker.Visible = true;
            this.Rootpnl.Visible = false;

        }

    }
}

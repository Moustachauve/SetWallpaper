using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SetWallpaper
{
    public partial class frmConnect : Form
    {
        private bool m_ipOk;
        private bool m_portOk;

        private Form1 m_parent;

        public frmConnect(Form1 parent)
        {
            InitializeComponent();
            m_parent = parent;

            m_ipOk = false; 
            m_portOk = false;
            UpdateButtonState();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            IPAddress address = IPAddress.Parse(txtIP.Text);
            int port = int.Parse(txtPort.Text);

            m_parent.Connect(address, port);
            Close();
        }

        private void UpdateButtonState()
        {
            btnConnect.Enabled = m_ipOk && m_portOk;
        }

        private void txtIP_TextChanged(object sender, EventArgs e)
        {
            IPAddress address;
            m_ipOk = IPAddress.TryParse(txtIP.Text, out address);
            UpdateButtonState();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int p;
            m_portOk = int.TryParse(txtPort.Text, out p);
            UpdateButtonState();
        }
    }
}

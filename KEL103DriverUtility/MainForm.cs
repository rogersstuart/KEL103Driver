using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KEL103DriverUtility
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var address = await KEL103Driver.KEL103Tools.FindLoadAddress();

            //textBox1.Text = address.ToString();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //get the ip address for the kel103 and begin polling
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //enable channel 2 monitor
        }
    }
}

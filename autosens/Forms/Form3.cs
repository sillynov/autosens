using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autosens.Forms
{
    public partial class Form3 : Form
    {
        public Form3(string messageOverride)
        {
            if(messageOverride != "blank")
            {
                InitializeComponent();
                label1.Text = messageOverride;
            }
            else
            {
                InitializeComponent();
            }
            textBox1.Text = Storage.userSettings.dpi.ToString();
            textBox2.Text = Storage.userSettings.steamProfileID;
            textBox3.Text = Storage.userSettings.defaultSens.ToString("0.0");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Storage.userSettings.steamProfileID = textBox2.Text;
            Storage.userSettings.dpi = int.Parse(textBox1.Text);
            Storage.userSettings.defaultSens = float.Parse(textBox3.Text);
            Storage.newUser = false;
            Storage.updateFilePaths();
            Storage.writeJson();
            Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}

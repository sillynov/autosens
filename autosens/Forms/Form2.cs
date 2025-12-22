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
    public partial class Form2 : Form
    {
        private string gameName;
        public Form2(string directoryMessage, string gameName)
        {
            InitializeComponent();
            label2.Text = directoryMessage;
            this.gameName = gameName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Game obj = Storage.gamesList.FirstOrDefault(g => g.name == gameName);
            if (obj != null)
            {
                obj.configPath = textBox2.Text;
                Storage.writeGamesList();
                MessageBox.Show("Directory updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
            }
        }
    }
}

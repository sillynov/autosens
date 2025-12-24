using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;

namespace autosens.Forms
{
    public partial class Form3 : Form
    {
        public Form3(string messageOverride)
        {
            if (messageOverride != "blank")
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
            Storage.UpdateFilePaths();
            Storage.WriteJson();
            Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\autosens\\Data\\";
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            steamIDHint.Text = Storage.steamPath + "\\userdata";
        }

        private void steamIDHint_MouseHover(object sender, EventArgs e)
        {
            steamIDHint.ForeColor = System.Drawing.Color.LightGray;
            Cursor = Cursors.Hand;
        }

        private void steamIDHint_MouseLeave(object sender, EventArgs e)
        {
            steamIDHint.ForeColor = System.Drawing.Color.Black;
            Cursor = Cursors.Default;
        }

        private void steamIDHint_Click(object sender, EventArgs e)
        {
            string path = Storage.steamPath + "\\userdata";
            System.Diagnostics.Process.Start("explorer.exe", path);
        }
    }
}

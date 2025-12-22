using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using autosens.Forms;

namespace autosens
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboGame.Items.Clear();
            comboGame.DataSource = Storage.gamesList;
            comboGame.DisplayMember = "name";
            comboGame.ValueMember = "conversionCalc";
            if(Storage.userSettings != null)
            {
                if (Storage.newUser)
                {
                    Form3 form3 = new Form3("We didn't find a usersettings.json file.\n You can fill out the boxes below, or not change them to leave them as default.");
                    form3.ShowDialog();
                }
                if ( Storage.userSettings.defaultSens != 0)
                {
                    textBox1.Text = Storage.userSettings.defaultSens.ToString("0.0");
                }
            }
            else
            {
                Form3 form3 = new Form3("We didn't find a usersettings.json file.\n You can fill out the boxes below, or not change them to leave them as default.");
                form3.ShowDialog();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Game obj = comboGame.SelectedItem as Game;
            if (obj != null)
            {
                Console.WriteLine("Selected game directory: " + obj.configPath);
                if(File.Exists(obj.configPath))
                {
                    Core.changeSensitivity(obj, float.Parse(textBox1.Text));
                }
                else {                     
                    string directoryErrorMessage = (obj.name + " configuration file not found: \"" + obj.configPath + ".\" Please enter the path to the config file below. \nYou can manually update this at \"" + Storage.jsonGamesPath + ".\" Check your user settings are up to date.");
                    Form2 form2 = new Form2(directoryErrorMessage, obj.name);
                    form2.ShowDialog();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3("Update user settings here.");
            form3.ShowDialog();
        }
    }
}

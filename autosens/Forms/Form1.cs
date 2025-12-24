using autosens.Forms;
using System;
using System.IO;
using System.Windows.Forms;

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
            if (Storage.userSettings != null)
            {
                if (Storage.newUser)
                {
                    Form3 form3 = new Form3("Create user settings.");
                    form3.ShowDialog();
                }
                if (Storage.userSettings.defaultSens != 0)
                {
                    textBox1.Text = Storage.userSettings.defaultSens.ToString("0.0");
                }
            }
            else
            {
                Form3 form3 = new Form3("User settings.");
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
                if (File.Exists(obj.configPath))
                {
                    Core.ChangeSensitivity(obj, float.Parse(textBox1.Text));
                    obj.currentSensitivity = Core.GetCurrentCm(obj);
                    Storage.WriteJson();
                    label3.Text = "Current: " + obj.currentSensitivity;
                }
                else
                {
                    string sensitvity = Core.CalculateSensitivity(obj, float.Parse(textBox1.Text)).ToString("0.0000");
                    string directoryErrorMessage = (obj.name + " configuration file not found: \"" + obj.configPath + ".\"  \nYou can manually update this path at \"" + Storage.jsonGamesPath + ".\" Check your user settings are up to date \nIf it's easier, your sensitivity should be " + sensitvity + ". (This will not work if your game is Battlefield V)");
                    MessageBox.Show(directoryErrorMessage);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3("Update user settings here.");
            form3.ShowDialog();
        }

        private void comboGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            Game obj = comboGame.SelectedItem as Game;
            label3.Text = "Current: " + obj.currentSensitivity;
            if (Storage.userSettings != null && Storage.userSettings.defaultSens != 0)
            {
                textBox1.Text = Storage.userSettings.defaultSens.ToString("0.0");
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            Game obj = comboGame.SelectedItem as Game;
            label3.Text = "Current: " + obj.currentSensitivity;
        }
    }
}

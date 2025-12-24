using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Text;

namespace autosens.Forms
{
    public partial class Form2 : Form
    {
        public Form2(Game game, float cm)
        {
            InitializeComponent();
            bool allowUpdate = game.allowUpdate;
            string errorMessage = game.notFoundText;
            if (!allowUpdate)
            {
                textBox1.Visible = false;
                button1.Text = "Close";
            }
            errorMessage = errorMessage.Replace("[SENS]", Core.CalculateSensitivity(game, cm).ToString("0.0###"));
            errorMessage = errorMessage.Replace("[PATH]", game.configPath);
            label1.Text = errorMessage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Visible)
            {
                string newPath = textBox1.Text;
                if (File.Exists(newPath))
                {
                    string currentGame = Storage.currentGameName;
                    foreach (Game game in Storage.gamesList)
                    {
                        Console.WriteLine("Checking game: " + game.name + " against " + currentGame);
                        if (game.name == currentGame)
                        {
                            Console.WriteLine(game.name + " found in games list. Updating config path...");
                            game.configPathTemplate = newPath;
                            game.configPath = newPath;
                            Console.WriteLine("Updated config path for " + game.name + " to: " + newPath);
                            Storage.WriteJson();
                            break;
                        }
                    }
                    Hide();
                }
                else
                {
                    MessageBox.Show("The specified file path does not exist. Please check the path and try again.");
                }
            }
            else
            {
                Hide();
            }
        }
    }
}

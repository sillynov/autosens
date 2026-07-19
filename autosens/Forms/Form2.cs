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
using NCalc;

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
            float sensitivity = Core.CalculateSensitivity(game, cm);
            errorMessage = errorMessage.Replace("[SENS]", sensitivity.ToString("0.0###"));
            errorMessage = errorMessage.Replace("[PATH]", game.configPath);

            if (!string.IsNullOrEmpty(game.displayCalc))
            {
                string calcString = game.displayCalc.Replace("[sens]", sensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture));
                try
                {
                    Expression e = new Expression(calcString);
                    string displayValue = Convert.ToSingle(e.Evaluate()).ToString("0.###");
                    errorMessage = errorMessage.Replace("[DISPLAYSENS]", displayValue);
                }
                catch { }
            }

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
                        if (game.name == currentGame)
                        {
                            game.configPathTemplate = newPath;
                            game.configPath = newPath;
                            game.userUpdatedPath = true;
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

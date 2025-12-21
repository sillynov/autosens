using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace autosens
{
    internal class Core
    {
        public static List<Games> gamesList;
        private static float sensitivity;
        [STAThread]
        public static void Main()
        {
            gamesList = First.gamesList;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            First.JsonSerialize();
        }

        public void Init()
        {
        }

        public void convertSensitivity(Games game, float cm)
        {
            sensitivity = Converter.FinalSensitivity(game, cm);
        }
    }
}

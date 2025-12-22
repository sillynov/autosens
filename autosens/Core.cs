using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using autosens.Forms;
using System.Data;

namespace autosens
{
    internal class Core
    {
        [STAThread]
        public static void Main()
        {
            Storage.initializeStorage();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static void changeSensitivity(Game game, float cm)
        {
            float sensitivity = CalculateSensitivity(game, cm);
            if(sensitivity == 0f)
            {
                return;
            }

            string configPath = game.configPath;
            string replacementText = game.replacementText;
            replaceFileContents(configPath, replacementText, sensitivity);
        }

        private static float CalculateSensitivity(Game game, float cm)
        {
            float sensitivity = 0f;
            string unprocessedExpression = game.conversionCalc;
            string expressionString = unprocessedExpression.Replace("[cm]", cm.ToString());
            object sens;
            try
            {
                sens = new DataTable().Compute(expressionString, null);
                sensitivity = Convert.ToSingle(sens);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating sensitivity: " + ex.Message);
                return 0f;
            }
            return sensitivity;
        }

        public static void replaceFileContents(string filePath, string searchText, float newValue)
        {
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if(fileExtension == ".sav")
            {
                replaceBinaryContents(filePath, searchText, newValue);
            }
            else
            {
                replaceTextContents(filePath, searchText, newValue);
            }
        }

        private static void replaceTextContents(string filePath, string searchText, float newValue)
        {
            string content = File.ReadAllText(filePath);

            string pattern = $@"({searchText}.*?)-?[0-9]+(\.[0-9]+)?";

            string oldNumber = "";

            string newContent = Regex.Replace(content, pattern, m =>
            {
                string prefix = m.Groups[1].Value;
                oldNumber = m.Groups[2].Value;

                string formattedValue;
                if (oldNumber.Contains("."))
                {
                    formattedValue = newValue.ToString("0.0###");
                }
                else
                {
                    formattedValue = ((int)newValue).ToString();
                }
                return prefix + formattedValue;
            });

            File.WriteAllText(filePath, newContent);
            MessageBox.Show("Sensitivity updated from " + oldNumber + " to " + newValue.ToString("0.0##"));
        }


        private static void replaceBinaryContents(string filePath, string searchText, float newValue)
        {
            List<byte> fileData = new List<byte>(File.ReadAllBytes(filePath));
            byte[] keyPattern = Encoding.ASCII.GetBytes(searchText);

            int keyIndex = findPattern(fileData, keyPattern);
            if (keyIndex == -1) { 
                return; 
            }

            int foundLengthHeaderIndex = -1;
            int foundStringLength = -1;
            string foundOldValue = "";

            int startScan = keyIndex + keyPattern.Length;
            for (int i = 0; i < 100; i++)
            {
                int currentIndex = startScan + i;
                if (currentIndex + 4 >= fileData.Count) break;

                int candidateLength = BitConverter.ToInt32(fileData.ToArray(), currentIndex);

                if (candidateLength > 0 && candidateLength < 20)
                {
                    int valueStart = currentIndex + 4;
                    if (valueStart + candidateLength < fileData.Count)
                    {
                        string candidateString = Encoding.ASCII.GetString(fileData.ToArray(), valueStart, candidateLength);
                        string cleanString = candidateString.TrimEnd('\0');

                        if (double.TryParse(cleanString, out _))
                        {
                            foundLengthHeaderIndex = currentIndex;
                            foundStringLength = candidateLength;
                            foundOldValue = cleanString;
                            break;
                        }
                    }
                }
            }

            if (foundLengthHeaderIndex != -1)
            {
                string finalValueToWrite = newValue.ToString();

                if (foundOldValue.Contains("."))
                {
                    string[] parts = foundOldValue.Split('.');
                    int oldDecimals = parts.Length > 1 ? parts[1].Length : 0;

                    if (double.TryParse(newValue.ToString(), out double valAsNum))
                    {
                        finalValueToWrite = valAsNum.ToString("F" + oldDecimals);
                        MessageBox.Show("Sensitivity updated from " + foundOldValue + " to " + finalValueToWrite);
                    }
                }
                else
                {
                    finalValueToWrite = ((int)double.Parse(newValue.ToString())).ToString();
                }

                byte[] newStringBytes = Encoding.ASCII.GetBytes(finalValueToWrite + "\0");
                int newLength = newStringBytes.Length;

                byte[] newLengthBytes = BitConverter.GetBytes(newLength);
                for (int k = 0; k < 4; k++)
                {
                    fileData[foundLengthHeaderIndex + k] = newLengthBytes[k];
                }

                fileData.RemoveRange(foundLengthHeaderIndex + 4, foundStringLength);
                fileData.InsertRange(foundLengthHeaderIndex + 4, newStringBytes);

                File.WriteAllBytes(filePath, fileData.ToArray());
            }
            else
            {
                MessageBox.Show("Could not find the sensitivity value in the binary file.");
            }
        }

        static int findPattern(List<byte> data, byte[] pattern)
        {
            for (int i = 0; i < data.Count - pattern.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (data[i + j] != pattern[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match) return i;
            }
            return -1;
        }
    }
}

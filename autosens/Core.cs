using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace autosens
{
    internal class Core
    {
        [STAThread]
        public static void Main()
        {
            Storage.InitializeStorage();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static void ChangeSensitivity(Game game, float cm)
        {
            float sensitivity = CalculateSensitivity(game, cm);
            if (sensitivity == 0f)
            {
                return;
            }

            string configPath = game.configPath;
            string replacementText = game.replacementText;
            ReplaceFileContents(configPath, replacementText, sensitivity);
        }

        public static float CalculateSensitivity(Game game, float cm)
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
            sensitivity = sensitivity * (1600f / Storage.userSettings.dpi);
            return sensitivity;
        }
        public static string FindConfigPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string[] pathParts = path.Split(new string[] { "[UNKNOWN]" }, StringSplitOptions.None);

            if (pathParts.Length < 2)
            {
                return path;
            }

            string part1 = pathParts[0];
            string part2 = pathParts[1];

            string baseDir = Path.GetDirectoryName(part1);
            if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
            {
                return path;
            }

            foreach (string dir in Directory.GetDirectories(baseDir))
            {
                string cleanedPart2 = part2.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string possiblePath = Path.Combine(dir, cleanedPart2);
                if (File.Exists(possiblePath))
                {
                    return possiblePath;
                }
            }

            return path;
        }

        public static void ReplaceFileContents(string filePath, string searchText, float newValue)
        {
            if(searchText.Contains("[AND]"))
            {
                string[] parts = searchText.Split(new string[] { "[AND]" }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    ReplaceFileContents(filePath, part, newValue);
                }
                return;
            }
            string fileExtension = Path.GetExtension(filePath).ToLower();
            if (fileExtension == ".sav")
            {
                ReplaceBinaryContents(filePath, searchText, newValue);
            }
            else
            {
                ReplaceTextContents(filePath, searchText, newValue);
            }
        }

        private static void ReplaceTextContents(string filePath, string searchText, float newValue)
        {
            string content = File.ReadAllText(filePath);

            string pattern = $@"({searchText}.*?)(-?[0-9]+(?:\.[0-9]+)?)";

            string oldNumber = "";

            string newContent = Regex.Replace(content, pattern, m =>
            {
                string prefix = m.Groups[1].Value;
                oldNumber = m.Groups[2].Value;

                string formattedValue;
                formattedValue = newValue.ToString("0.0###", CultureInfo.GetCultureInfo("en-US"));
                return prefix + formattedValue;
            });
            if(oldNumber == "")
            {
                MessageBox.Show("Could not find the sensitivity value in the config file.");
                return;
            }
            else
            {
                File.WriteAllText(filePath, newContent);
                MessageBox.Show("Sensitivity updated from " + oldNumber + " to " + newValue.ToString("0.0##"));
            }
        }

        private static void ReplaceBinaryContents(string filePath, string searchText, float newValue)
        {
            List<byte> fileData = new List<byte>(File.ReadAllBytes(filePath));
            byte[] keyPattern = Encoding.ASCII.GetBytes(searchText);

            int keyIndex = FindPattern(fileData, keyPattern);
            if (keyIndex == -1)
            {
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
                //:(
            }
        }

        static int FindPattern(List<byte> data, byte[] pattern)
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

        public static string GetCurrentCm(Game game)
        {
            float currentSens = 0;
            string fileExtension = Path.GetExtension(game.configPath).ToLower();
            string searchText = game.replacementText;
            if(searchText.Contains("[AND]"))
            {
                string[] parts = searchText.Split(new string[] { "[AND]" }, StringSplitOptions.None);
                foreach (string part in parts)
                {
                    game.replacementText = part;
                    string sensString = GetCurrentCm(game);
                    if(sensString != "Not Found")
                    {
                        game.replacementText = searchText;
                        return sensString;
                    }
                }
                game.replacementText = searchText;
                return "Not Found";
            }
            if (fileExtension == ".sav")
            {
                try
                {
                    currentSens = OldSensBinary(game.configPath, game.replacementText);
                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    Console.WriteLine("Reading config for " + game.name + " at " + game.configPath);
                    currentSens = GetCurrentSensCfg(game.configPath, game.replacementText);
                }
                catch
                {

                }
            }

            if (currentSens == 0f)
            {
                return "Not Found";
            }

            float finalCm = 0f;
            string unprocessedExpression = game.reverseCalc;
            string expressionString = unprocessedExpression.Replace("[sens]", currentSens.ToString());
            object cm;
            try
            {
                cm = new DataTable().Compute(expressionString, null);
                finalCm = Convert.ToSingle(cm);
                finalCm = finalCm * (1600f / Storage.userSettings.dpi);
            }
            catch
            {
                return "Not Found";
            }
            return finalCm.ToString("0.0");
        }

        private static float OldSensBinary(string filePath, string searchText)
        {
            List<byte> fileData = new List<byte>(File.ReadAllBytes(filePath));
            byte[] keyPattern = Encoding.ASCII.GetBytes(searchText);

            int keyIndex = FindPattern(fileData, keyPattern);
            if (keyIndex == -1)
            {
                return 0f;
            }

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
                            foundOldValue = cleanString;
                            return float.Parse(foundOldValue);
                        }
                    }
                }
            }
            return 0f;
        }

        private static float GetCurrentSensCfg(string filePath, string searchText)
        {
            string content = File.ReadAllText(filePath);

            string pattern = $@"{Regex.Escape(searchText)}.*?(-?[0-9]+(?:[.,][0-9]+)?)";

            Match match = Regex.Match(content, pattern);

            if (match.Success)
            {
                string numberString = match.Groups[1].Value.Replace(",", ".");

                Console.WriteLine("Found current sensitivity: " + numberString);
                return float.Parse(numberString);
            }
            return 0;
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;

namespace autosens
{
    internal class Storage
    {
        private static string jsonGamesString;
        private static string jsonUserSettingsString;
        public static string jsonUserSettingsPath = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\userSettings.json";
        public static string jsonGamesPath = AppDomain.CurrentDomain.BaseDirectory + "\\Data\\games.json";
        public static List<Game> gamesList;
        public static User userSettings;
        public static bool newUser = true;
        private static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static void initializeStorage()
        {
            if (File.Exists(jsonGamesPath))
            {
                readGamesList();
            }
            else
            {
                createGamesList();
            }

            if (File.Exists(jsonUserSettingsPath))
            {
                readUserSettings();
            }
            else
            {
                createUserSettings();
            }
            updateFilePaths();
            updateCurrentSensitivities();
            writeJson();
        }

        public static void readGamesList()
        {
            jsonGamesString = File.ReadAllText(jsonGamesPath);
            try
            {
                gamesList = JsonSerializer.Deserialize<List<Game>>(jsonGamesString);
            } 
            catch (Exception e) 
            {
                MessageBox.Show("Error reading games list: " + e.Message + "\nReverting to default list. Original version saved at " + AppDomain.CurrentDomain.BaseDirectory + "\\Data\\gamesbackup.json");
                File.Copy(jsonGamesPath, AppDomain.CurrentDomain.BaseDirectory + "\\Data\\gamesbackup.json", true);
                createGamesList();
            }
            gamesList.Sort((x, y) => x.name.CompareTo(y.name));
        }
        public static void writeGamesList()
        {
            gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            jsonGamesString = JsonSerializer.Serialize(gamesList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonGamesPath, jsonGamesString);
        }

        public static void readUserSettings()
        {
            jsonUserSettingsString = File.ReadAllText(jsonUserSettingsPath);
            try
            {
                userSettings = JsonSerializer.Deserialize<User>(jsonUserSettingsString);
                newUser = false;
            } 
            catch (Exception e)
            {
                MessageBox.Show("Error reading user settings: " + e.Message + "\nReverting to default settings. Original version saved at " + AppDomain.CurrentDomain.BaseDirectory + "\\Data\\usersettingsbackup.json");
                File.Copy(jsonUserSettingsPath, AppDomain.CurrentDomain.BaseDirectory + "\\Data\\usersettingsbackup.json", true);
                createUserSettings();
            }
            userSettings = JsonSerializer.Deserialize<User>(jsonUserSettingsString);
        }
        public static void writeUserSettings()
        {
            jsonUserSettingsString = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonUserSettingsPath, jsonUserSettingsString);
        }

        public static void writeJson()
        {
            writeGamesList();
            writeUserSettings();
        }

        public static void updateFilePaths()
        {
            foreach (Game game in gamesList)
            {
                string configPath = game.configPathTemplate;
                configPath = configPath.Replace("[APPDATA]", appDataPath);
                configPath = configPath.Replace("[LOCALAPPDATA]", localAppDataPath);
                configPath = configPath.Replace("[DOCUMENTS]", documentsPath);
                configPath = configPath.Replace("[STEAMID]", userSettings.steamProfileID);
                game.configPath = configPath;
            }
        }

        private static void createGamesList()
        {
            gamesList = new List<Game>
                {
                    new Game { name = "The Finals", conversionCalc = "571.5 / [cm]", reverseCalc = "571.5 / [sens]", configPathTemplate = "[LOCALAPPDATA]\\Discovery\\Saved\\SaveGames\\EmbarkOptionSaveGame.sav", replacementText = "MouseSensitivity", configPath = " ", currentSensitivity = "0.0"},
                    new Game { name = "Counterstrike 2", conversionCalc = "25.977 / [cm]", reverseCalc = "25.977 / [sens]", configPathTemplate = "C:\\Program Files (x86)\\Steam\\userdata\\[STEAMID]\\730\\local\\cfg\\cs2_user_convars_0_slot0.vcfg", replacementText = "\"sensitivity\"", configPath = " ", currentSensitivity = "0.0"},
                    new Game { name = "Battlefield V", conversionCalc = "25.977 / [cm]", reverseCalc = "25.977 / [sens]", configPathTemplate = "[DOCUMENTS]\\Battlefield V\\settings\\PROFSAVE_profile_synced", replacementText = "GstInput.MouseSensitivity ", configPath = " ", currentSensitivity = "0.0"},
                    new Game { name = "Battlefield 4", conversionCalc = "25.977 / [cm]", reverseCalc = "25.977 / [sens]", configPathTemplate = "[DOCUMENTS]\\Battlefield 4\\settings\\PROFSAVE_profile", replacementText = "GstInput.MouseSensitivity ", configPath = " ", currentSensitivity = "0.0"}
                };
        }

        private static void createUserSettings()
        {
            userSettings = new User { dpi = 1600, steamProfileID = "0", defaultSens = 0.0f };
        }

        private static void updateCurrentSensitivities()
        {
            foreach (Game game in gamesList)
            {
                if (File.Exists(game.configPath))
                {
                    game.currentSensitivity = Core.currentCm(game);
                }
                else
                {
                    game.currentSensitivity = "Config file not found";
                }
            }
        }
    }
}

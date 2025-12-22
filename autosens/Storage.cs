using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

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
                gamesList = new List<Game>
                {
                    new Game { name = "The Finals", conversionCalc = "571.5 / [cm]", configPathTemplate = "[LOCALAPPDATA]\\Discovery\\Saved\\SaveGames\\EmbarkOptionSaveGame.sav", replacementText = "MouseSensitivity", configPath = " " },
                    new Game { name = "Counterstrike 2", conversionCalc = "25.977 / [cm]", configPathTemplate = "C:\\Program Files (x86)\\Steam\\userdata\\[STEAMID]\\730\\local\\cfg\\cs2_user_convars_0_slot0.vcfg", replacementText = "\"sensitivity\"", configPath = " "}
                };
            }

            if (File.Exists(jsonUserSettingsPath))
            {
                readUserSettings();
                newUser = false;
            }
            else
            {
                userSettings = new User { dpi = 1600, steamProfileID = "0", defaultSens = 0.0f };
            }
            updateFilePaths();
            writeJson();
        }
        public static void readGamesList()
        {
            jsonGamesString = File.ReadAllText(jsonGamesPath);
            gamesList = JsonSerializer.Deserialize<List<Game>>(jsonGamesString);
        }
        public static void writeGamesList()
        {
            jsonGamesString = JsonSerializer.Serialize(gamesList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonGamesPath, jsonGamesString);
        }

        public static void readUserSettings()
        {
            jsonUserSettingsString = File.ReadAllText(jsonUserSettingsPath);
            userSettings = JsonSerializer.Deserialize<User>(jsonUserSettingsString);
        }
        public static void writeUserSettings()
        {
            jsonUserSettingsString = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonUserSettingsPath, jsonUserSettingsString);
        }

        public static void writeJson()
        {
            jsonGamesString = JsonSerializer.Serialize(gamesList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonGamesPath, jsonGamesString);
            jsonUserSettingsString = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonUserSettingsPath, jsonUserSettingsString);
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
    }
}

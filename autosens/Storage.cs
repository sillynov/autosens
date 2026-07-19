using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using AutoUpdaterDotNET;

namespace autosens
{
    internal class Storage
    {
        private static string jsonGamesString;
        private static string jsonUserSettingsString;
        public static List<Game> gamesList;
        private static List<Game> tempList;
        public static User userSettings;
        public static bool newUser = true;
        private static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string localLowPath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%Low");
        public static string steamPath = GetSteamPath();
        public static string jsonUserSettingsPath = localAppDataPath + "\\autosens\\Data\\userSettings.json";
        public static string jsonGamesPath = localAppDataPath + "\\autosens\\Data\\games.json";
        public static string version = "1.5.0";
        public static string currentGameName = "";

        public static void InitializeStorage()
        {
            AutoUpdater.Start("https://raw.githubusercontent.com/sillynov/autosens/refs/heads/master/update.xml");
            if (File.Exists(jsonGamesPath))
            {
                ReadGamesList();
            }
            else
            {
                CreateGamesList();
            }

            if (File.Exists(jsonUserSettingsPath))
            {
                ReadUserSettings();
            }
            else
            {
                CreateUserSettings();
            }
            if(userSettings.version != version)
            {
                userSettings.version = version;
                MessageBox.Show("Welcome to autosens v" + version + "! Your game settings have been updated. \nIf you need it, your old settings are backed up at " + localAppDataPath + "\\autosens\\Data\\gamesbackup.json");
                tempList = gamesList;
                File.Copy(jsonGamesPath, localAppDataPath + "\\autosens\\Data\\gamesbackup.json", true);
                CreateGamesList();
                foreach (Game oldGame in tempList)
                {
                    bool found = false;
                    foreach (Game newGame in gamesList)
                    {
                        if (oldGame.name == newGame.name)
                        {
                            if (oldGame.userUpdatedPath)
                            {
                                newGame.configPathTemplate = oldGame.configPathTemplate;
                                newGame.userUpdatedPath = true;
                                found = true;
                                break;
                            }
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        gamesList.Add(oldGame);
                    }
                }
            }
            UpdateFilePaths();
            UpdateCurrentSensitivities();
            WriteJson();
        }

        public static void ReadGamesList()
        {
            jsonGamesString = File.ReadAllText(jsonGamesPath);
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                gamesList = JsonSerializer.Deserialize<List<Game>>(jsonGamesString, options);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error reading games list: " + e.Message + "\nReverting to default list. Original version saved at " + localAppDataPath + "\\Data\\gamesbackup.json");
                File.Copy(jsonGamesPath, localAppDataPath + "\\autosens\\Data\\gamesbackup.json", true);
                CreateGamesList();
            }
            gamesList.Sort((x, y) => x.name.CompareTo(y.name));
        }
        public static void WriteGamesList()
        {
            gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            var filteredGamesList = gamesList.Select(obj => new
            {
                obj.name,
                obj.conversionCalc,
                obj.reverseCalc,
                obj.configPathTemplate,
                obj.replacementText,
                obj.notFoundText,
                obj.allowUpdate,
                obj.displayCalc,
                obj.userUpdatedPath
            }).ToList();

            jsonGamesString = JsonSerializer.Serialize(filteredGamesList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonGamesPath, jsonGamesString);
        }

        public static void ReadUserSettings()
        {
            jsonUserSettingsString = File.ReadAllText(jsonUserSettingsPath);
            try
            {
                userSettings = JsonSerializer.Deserialize<User>(jsonUserSettingsString);
                newUser = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Error reading user settings: " + e.Message + "\nReverting to default settings. Original version saved at " + localAppDataPath + "\\autosens\\Data\\usersettingsbackup.json");
                File.Copy(jsonUserSettingsPath, localAppDataPath + "\\autosens\\Data\\usersettingsbackup.json", true);
                CreateUserSettings();
            }
            userSettings = JsonSerializer.Deserialize<User>(jsonUserSettingsString);
        }
        public static void WriteUserSettings()
        {
            jsonUserSettingsString = JsonSerializer.Serialize(userSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonUserSettingsPath, jsonUserSettingsString);
        }

        public static void WriteJson()
        {
            WriteGamesList();
            WriteUserSettings();
        }

        public static void UpdateFilePaths()
        {
            foreach (Game game in gamesList)
            {
                string configPath = game.configPathTemplate;
                configPath = configPath.Replace("[APPDATA]", appDataPath);
                configPath = configPath.Replace("[LOCALAPPDATA]", localAppDataPath);
                configPath = configPath.Replace("[LOCALLOW]", localLowPath);
                configPath = configPath.Replace("[DOCUMENTS]", documentsPath);
                configPath = configPath.Replace("[STEAMID]", userSettings.steamProfileID);
                configPath = configPath.Replace("[STEAM]", steamPath);
                if (configPath.Contains("[UNKNOWN]"))
                {
                    configPath = Core.FindConfigPath(configPath);
                }
                game.configPath = configPath;
            }
        }

        private static void CreateGamesList()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("autosens.games.json"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    gamesList = JsonSerializer.Deserialize<List<Game>>(json);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("failed to load embedded games list: " + ex.Message);
                gamesList = new List<Game>();
            }

            Directory.CreateDirectory(localAppDataPath + "\\autosens\\Data\\");
            WriteGamesList();
        }

        private static void CreateUserSettings()
        {
            userSettings = new User { dpi = 1600, steamProfileID = "0", defaultSens = 0.0f, version = version };
            Directory.CreateDirectory(localAppDataPath + "\\autosens\\Data\\");
            WriteUserSettings();
        }

        private static void UpdateCurrentSensitivities()
        {
            foreach (Game game in gamesList)
            {
                Console.WriteLine("stupid " + game.conversionCalc);
                if (File.Exists(game.configPath))
                {
                    game.currentSensitivity = Core.GetCurrentCm(game);
                }
                else
                {
                    game.currentSensitivity = "Config file not found";
                }
            }
        }

        //hytioo work, i should probably figure out how it works if i decide to change anything
        private static string GetSteamPath()
        {
            string defaultPath = "C:\\Program Files (x86)\\Steam";
            string steamPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath", defaultPath) as string;

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam",
                    "InstallPath",
                    defaultPath) as string;
            }

            return steamPath;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace autosens
{
    internal class First
    {
        private static string jsonString;
        public static List<Games> gamesList = new List<Games>
        {
            new Games { name = "The Finals", conversionFactor = 3.14, configDirectory = "C:\\Users\\nov\\AppData\\Local\\Discovery\\Saved\\Config\\WindowsClient\\GameUserSettings.ini"},
            new Games { name = "Battlefield V", conversionFactor = 3.44 },
            new Games { name = "Counterstrike 2", conversionFactor = 2.54 }
        };
        public static void JsonSerialize()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            jsonString = JsonSerializer.Serialize(gamesList, options);
            Console.WriteLine(jsonString);
        }   
    }
}

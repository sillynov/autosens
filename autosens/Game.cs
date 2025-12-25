namespace autosens
{
    public class Game
    {
        public string name { get; set; }
        public string conversionCalc { get; set; }
        public string reverseCalc { get; set; }
        public string replacementText { get; set; }
        public string configPathTemplate { get; set; }
        public string configPath { get; set; }
        public string currentSensitivity { get; set; }
        public string notFoundText { get; set; }
        public bool allowUpdate { get; set; }
    }
}

using CommandLine;

namespace CSGORUN_Robot
{
    public class CommandLineOptions
    {
        [Option("settings-filepath", HelpText = "Prefer another settings file than default.")] 
        public string SettingsFilepath { get; set; }
    }
}
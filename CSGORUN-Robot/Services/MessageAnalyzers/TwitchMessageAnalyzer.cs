using CSGORUN_Robot.Settings;
using CSGORUN_Robot.Twitch.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public class TwitchMessageAnalyzer : IMessageАnalyzer
    {
        private UserMessage _message;
        public TwitchMessageAnalyzer(UserMessage msg)
        {
            _message = msg;
        }

        public IEnumerable<string> Analyze()
        {
            if (_message.Channel.Equals(_message.Nickname, System.StringComparison.OrdinalIgnoreCase))
            {
                var pattern = AppSettingsProvider.Provide().Twitch.RegexPattern;
                return Regex.Matches(_message.Message.Trim(), pattern)
                    .Cast<Match>()
                    .Select(t => t.Value);
            }

            return null;
        }
    }
}

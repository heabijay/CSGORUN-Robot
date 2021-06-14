using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public class TwitchMessageAnalyzer : MessageAnalyzerBase<TwitchMessageWrapper>
    {
        private protected override IEnumerable<string> Analyze(TwitchMessageWrapper message)
        {
            var msg = message.Message;

            if (msg.Channel.Equals(msg.Nickname, System.StringComparison.OrdinalIgnoreCase))
            {
                var pattern = AppSettingsProvider.Provide().Twitch.RegexPattern;
                return Regex.Matches(msg.Message.Trim(), pattern)
                    .Cast<Match>()
                    .Select(t => t.Value);
            }

            return null;
        }
    }
}

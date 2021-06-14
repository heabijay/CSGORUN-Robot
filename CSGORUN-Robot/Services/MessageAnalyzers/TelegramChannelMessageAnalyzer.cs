using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public class TelegramChannelMessageAnalyzer : MessageAnalyzerBase<TelegramChannelMessageWrapper>
    {
        private protected override IEnumerable<string> Analyze(TelegramChannelMessageWrapper message)
        {
            var settings = AppSettingsProvider.Provide().Telegram.Aggregator.Channels;
            var channelUsername = message.Message.Channel.Username.FirstOrDefault();
            var channelPreset = settings.FirstOrDefault(t => t.Username.TrimStart('@').Equals(channelUsername, System.StringComparison.OrdinalIgnoreCase));

            if (channelPreset != null)
            {
                return Regex.Matches(message.Message.Message.Message, channelPreset.Regex)
                    .Cast<Match>()
                    .Select(t => t.Value);
            }

            return null;
        }
    }
}

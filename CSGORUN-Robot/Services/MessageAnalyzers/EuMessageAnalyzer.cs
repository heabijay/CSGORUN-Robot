using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public class EuMessageAnalyzer : MessageAnalyzerBase<EuMessageWrapper>
    {
        private protected override IEnumerable<string> Analyze(EuMessageWrapper message)
        {
            var msg = message.Message;

            //if (text.StartsWith("@")) text = text.Substring(message.IndexOf(',') + 1);
            if (!IsExclusion(msg))
            {
                var patterns = AppSettingsProvider.Provide().CSGORUN.RegexPatterns;
                var pattern = msg.user.role >= 4 ? patterns.EN_Admins : patterns.Default;

                return Regex.Matches(msg.message, pattern)
                    .Cast<Match>()
                    .Select(t => t.Value);
            }

            return null;
        }

        private static bool IsExclusion(ChatPayload msg)
        {
            var text = msg.message;

            if (text.StartsWith("@") && text.IndexOf(',') > -1)
                return true;

            if ((text.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) ||
                (text.StartsWith("data:image/", System.StringComparison.OrdinalIgnoreCase) && text.Contains(";base64,", System.StringComparison.OrdinalIgnoreCase)))
                && !text.Contains(' '))
                return true;

            return false;
        }
    }
}

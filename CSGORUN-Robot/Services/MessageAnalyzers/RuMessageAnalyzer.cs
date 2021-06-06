using CSGORUN_Robot.CSGORUN.WebSocket_DTOs;
using CSGORUN_Robot.Services.MessageWrappers;
using CSGORUN_Robot.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    public class RuMessageAnalyzer : MessageAnalyzerBase<RuMessageWrapper>
    {
        private protected override IEnumerable<string> Analyze(RuMessageWrapper message)
        {
            var msg = message.Message;

            //if (text.StartsWith("@")) text = text.Substring(message.IndexOf(',') + 1);
            if (!IsExclusion(msg))
            {
                var patterns = AppSettingsProvider.Provide().CSGORUN.RegexPatterns;
                var isAdmin = msg.user.role >= 4;
                var pattern = isAdmin ? patterns.RU_Admins : patterns.Default;

                IEnumerable<string> results = Enumerable.Empty<string>();
                if (isAdmin)
                {
                    var text = msg.message;

                    // Special promo decrypt (of anti-bot method)
                    var ltext = text.ToLower();
                    var isMatch = Regex.IsMatch(ltext, "((?<![А-Яа-яA-Za-z0-9])точк)|((?<![А-Яа-яA-Za-z0-9])убери)|((?<![А-Яа-яA-Za-z0-9])убира)|((?<![А-Яа-яA-Za-z0-9])убрать((?![А-Яа-яA-Za-z0-9])))|((?<![А-Яа-яA-Za-z0-9])удал)");
                    
                    if (isMatch)
                    {
                        var isContainsDot = text.Contains('.');
                        var isContainsComma = text.Contains(',');

                        if (isContainsDot)
                        {
                            var messageWoDots = text.Replace(".", "");
                            results = results.Concat(
                                Regex.Matches(messageWoDots, pattern)
                                    .Cast<Match>()
                                    .Select(t => t.Value)
                                );
                        }

                        if (isContainsComma)
                        {
                            var messageWoComma = text.Replace(",", "");
                            results = results.Concat(
                                Regex.Matches(messageWoComma, pattern)
                                    .Cast<Match>()
                                    .Select(t => t.Value)
                                );
                        }
                    }
                }

                results = results.Concat(
                    Regex.Matches(msg.message, pattern)
                        .Cast<Match>()
                        .Select(t => t.Value)
                    );

                return results;
            }

            return null;
        }

        private static bool IsExclusion(ChatPayload msg)
        {
            var text = msg.message;

            if (text.StartsWith("@") && text.IndexOf(',') > -1)
                return true;

            if ((text.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                (text.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && text.Contains(";base64,", System.StringComparison.OrdinalIgnoreCase)))
                && !text.Contains(' '))
                return true;

            return false;
        }
    }
}

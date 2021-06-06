using System.Collections.Generic;

namespace CSGORUN_Robot.Services.MessageAnalyzers
{
    /// <summary>
    /// Analyzes messages for exists promo-codes
    /// </summary>
    public interface IMessageАnalyzer
    {
        /// <summary>
        /// Performs analyze of message
        /// </summary>
        /// <returns>IEnumerable of promo-codes found</returns>
        public IEnumerable<string> Analyze();
    }
}

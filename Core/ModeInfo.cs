using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BoveeBot.Core
{
    public class ModeInfo
    {
        public string Mode { get; set; }
        public int MatchesLeft { get; set; }
        public string TournamentTitle { get; set; }
        public ModeInfo (string mode, int matchesLeft, string tournamentTitle)
        {
            Mode = mode;
            MatchesLeft = matchesLeft;
            TournamentTitle = tournamentTitle;
        }
    }
}
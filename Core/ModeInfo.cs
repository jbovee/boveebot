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
        public string TournamentType { get; set; }
        public string TournamentTitle { get; set; }
        public ModeInfo (string mode, int matchesLeft, string tournamentType, string tournamentTitle)
        {
            Mode = mode;
            MatchesLeft = matchesLeft;
            TournamentType = tournamentType;
            TournamentTitle = tournamentTitle;
        }
    }
}
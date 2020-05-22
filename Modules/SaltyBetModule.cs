using System;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Html.Parser;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BoveeBot.Core;

namespace BoveeBot.Modules
{
    [Group("saltybet"), Alias("sb"), Name("SaltyBet")]
    public class SaltyBet : ModuleBase<SocketCommandContext>
    {
        private readonly DatabaseService _database;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private Regex modeTextRegex = new Regex(@"[^!]*", RegexOptions.Compiled);
        private Regex numsCheckRegex = new Regex(@"[0-9]{1,3}", RegexOptions.Compiled);
        private Regex exhibCheckRegex = new Regex(@"([0-9]{1,3} exhibition matches left|Matchmaking mode will be activated after the next exhibition match)", RegexOptions.Compiled);
        private Regex mmCheckRegex = new Regex(@"([0-9]{1,3} more matches until the next tournament|Tournament mode will be activated after the next match)", RegexOptions.Compiled);
        private Regex tourneyCheckRegex = new Regex(@"((Tournament mode start)|[0-9]{1,2} characters are left in the bracket|FINAL ROUND! Stay tuned for exhibitions after the tournament)", RegexOptions.Compiled);

        public SaltyBet(
            DatabaseService database,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _database = database;
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;
        }

        [Command]
        public async Task ShowSaltyBetStatus()
        {
            var currentModeInfo = await GetModeInfo();

            var msg = $"**Mode and Matches Left**\n" +
                      $"{currentModeInfo.Mode}    {currentModeInfo.MatchesLeft}";
            if (String.Equals(currentModeInfo.Mode, "Tournament"))
            {
                msg += $"\n**{currentModeInfo.TournamentType}**";
                if (!String.IsNullOrEmpty(currentModeInfo.TournamentTitle))
                {
                    msg += $"\n**{currentModeInfo.TournamentTitle}**";
                }
            } else
            {
                var minutesLeft = currentModeInfo.MatchesLeft * Double.Parse(_config["avgMatchLength"]);
                msg += $"\n**Approximate time before next mode**\n{String.Format("{0} -> {1}", minutesToApproxHrMin(minutesLeft), DateTime.Now.AddMinutes(minutesLeft).ToString("h:mm tt"))}";
            }
            
            await ReplyAsync(msg);
        }

        private async Task<ModeInfo> GetModeInfo()
        {
            var client = new WebClient();
            var text = client.DownloadString("https://www.saltybet.com/shaker?bracket=1");
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(text));
            var tournamentTypeRaw = document.QuerySelectorAll("div#compendiumright strong").FirstOrDefault().TextContent;
            var type = Regex.Match(tournamentTypeRaw, @"(\w+(?: \w+)?) Tournament Bracket").Groups[1].Value;
            var title = String.Equals(type, "Custom") ? document.QuerySelectorAll("div#compendiumright span").LastOrDefault().TextContent : "";
            var modeHtml = document.QuerySelectorAll("div#compendiumleft div").LastOrDefault().InnerHtml;

            var modeText = modeTextRegex.Match(modeHtml).Value;

            var exhibCheck = exhibCheckRegex.Match(modeText);
            var mmCheck = mmCheckRegex.Match(modeText);
            var tourneyCheck = tourneyCheckRegex.Match(modeText);

            var numsCheck = numsCheckRegex.Match(modeText);
            var matchNums = numsCheck.Success ? Int32.Parse(numsCheck.Value) : !String.IsNullOrEmpty(tourneyCheck.Groups[1].Value) ? 16 : 1;
            var currentMode = exhibCheck.Success ? "Exhibitions" : mmCheck.Success ? "Matchmaking" : tourneyCheck.Success ? "Tournament" : "Unknown";
            var info = new ModeInfo(currentMode, matchNums, type, title);
            return info;
        }

        private String minutesToApproxHrMin(double minutes)
        {
            var h = Math.Floor(minutes / 60);
            var m = Math.Floor(minutes - (h * 60));
            if (h == 0) return String.Format("~{0}m", m);
            return String.Format("~{0}h {1}m", h, m);
        }
    }
}

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
        private static string validateMsg = "does not match format requirements:\n\t- Between 3 and 12 characters\n\t- Using only letters with a single hyphen or apostrophe within them";

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
            //-- Exhibitions --
            // in: 24 exhibition matches left!
            // end: Matchmaking mode will be activated after the next exhibition match!
            // regex: exhibition match
            //-- Matchmaking --
            // in: 85 more matches until the next tournament!
            // end: Tournament mode will be activated after the next match!
            // regex: more matches or next match
            //-- Tournament --
            // in: Tournament mode start!
            // in: 3 character are left in the bracket!
            // end: FINAL ROUND! Stay tuned for exhibitions after the tournament!
            // regex: bracket or FINAL ROUND
            var client = new WebClient();
            var text = client.DownloadString("https://www.saltybet.com/shaker?bracket=1");
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(text));
            var modeHtml = document.QuerySelectorAll("div#compendiumleft div").LastOrDefault().InnerHtml;

            var modeText = Regex.Match(modeHtml, @"[^!]*").Value;
            var numsCheck = Regex.Match(modeText, @"[0-9]{1,3}");
            var matchNums = numsCheck.Success ? Int32.Parse(numsCheck.Value) : 1;

            var exhibCheck = Regex.Match(modeText, @"([0-9]{1,3} exhibition matches left|Matchmaking mode will be activated after the next exhibition match)");
            var mmCheck = Regex.Match(modeText, @"([0-9]{1,3} more matches until the next tournament|Tournament mode will be activated after the next match)");
            var tourneyCheck = Regex.Match(modeText, @"((Tournament mode start)|[0-9]{1,2} characters are left in the bracket|FINAL ROUND! Stay tuned for exhibitions after the tournament)");

            if (!String.IsNullOrEmpty(tourneyCheck.Groups[1].Value)) matchNums = 16;
            var currentMode = exhibCheck.Success ? "Exhibitions" : mmCheck.Success ? "Matchmaking" : tourneyCheck.Success ? "Tournament" : "Unknown";

            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
            };
            builder.AddField(x => {
                x.Name = "Current mode";
                x.Value = currentMode;
            });
            builder.AddField(x => {
                x.Name = "Matches left";
                x.Value = matchNums;
            });
            if (!String.Equals(currentMode, "Tournament"))
            {
                builder.AddField(x => {
                    x.Name = "Approximate time before next tournament";
                    x.Value = secondsToTime(matchNums * 2.7 * 60);
                });
            }
            
            await ReplyAsync("", false, builder.Build());
        }

        private String secondsToTime(double seconds)
        {
            var h = Math.Floor(seconds / 3600);
            var m = Math.Floor((seconds - (h * 3600)) / 60);
            if (h == 0) return String.Format("~{0}m", m);
            return String.Format("~{0}h {1}m", h, m);
        }
    }
}

using System;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using AngleSharp;
using System.Threading.Tasks;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client.Enums;
using Microsoft.Extensions.Configuration;
using BoveeBot.Core;

namespace BoveeBot
{
    public class SaltyBetService
    {
        private readonly DatabaseService _database;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private Regex untilNextTourneyRegex = new Regex(@"Payouts to Team (?:Red|Blue)\. ([0-9]{1,3}) more matches until the next tournament", RegexOptions.Compiled);
        private Regex tourneyStartingRegex = new Regex(@"Tournament will start shortly", RegexOptions.Compiled);
        private Regex modeTextRegex = new Regex(@"[^!]*", RegexOptions.Compiled);
        private Regex numsCheckRegex = new Regex(@"[0-9]{1,3}", RegexOptions.Compiled);
        private Regex exhibCheckRegex = new Regex(@"([0-9]{1,3} exhibition matches left|Matchmaking mode will be activated after the next exhibition match)", RegexOptions.Compiled);
        private Regex mmCheckRegex = new Regex(@"([0-9]{1,3} more matches until the next tournament|Tournament mode will be activated after the next match)", RegexOptions.Compiled);
        private Regex tourneyCheckRegex = new Regex(@"((Tournament mode start)|[0-9]{1,2} characters are left in the bracket|FINAL ROUND! Stay tuned for exhibitions after the tournament)", RegexOptions.Compiled);
        private ulong channelId = 409517124038426646;
        private string saltyRole = "<@&706951074753544225>";
        private readonly string _botName = "sbdiscordbot";
        private readonly string _broadcasterName = "saltybet";
        private readonly string _twitchOAuth;
        TwitchClient client;

        public SaltyBetService(
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
            _twitchOAuth = _config["twitchOAuth"];

            ConnectionCredentials credentials = new ConnectionCredentials(_botName, _twitchOAuth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient();
            client.Initialize(credentials, _broadcasterName);

            client.OnConnected += Client_OnConnected;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;

            client.Connect();
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }
  
        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Joined channel");
        }

        private async void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            switch(e.ChatMessage.Username) {
                case "waifu4u":
                    var untilNextTourney = untilNextTourneyRegex.Match(e.ChatMessage.Message);
                    if (untilNextTourney.Success)
                    {
                        var matchesLeft = Int32.Parse(untilNextTourney.Groups[1].Value);
                        if (matchesLeft == 5)
                        {
                            var chnl = _discord.GetChannel(channelId) as IMessageChannel;
                            await chnl.SendMessageAsync(String.Format("{0} {1} matches left until the next tournament!", saltyRole, matchesLeft));
                        }
                    }
                    break;
                case "saltybet":
                    if (tourneyStartingRegex.Match(e.ChatMessage.Message).Success)
                    {
                        var tourneyInfo = await GetModeInfo();
                        var chnl = _discord.GetChannel(channelId) as IMessageChannel;
                        var tourneyMsg = $"{saltyRole} Tournament starting soon! **{tourneyInfo.TournamentType}**";
                        if (!String.IsNullOrEmpty(tourneyInfo.TournamentTitle)) tourneyMsg += $" - **{tourneyInfo.TournamentTitle}**";

                        var saltyLinks = new EmbedBuilder
                        {
                            Description = "[SaltyBet](https://www.saltybet.com/) | [Bracket](https://www.saltybet.com/shaker?bracket=1)"
                        }.Build();
                        await chnl.SendMessageAsync(tourneyMsg);
                        await chnl.SendMessageAsync(embed: saltyLinks);
                    }
                    break;
            }
        }

        private async Task<ModeInfo> GetModeInfo()
        {
            var client = new WebClient();
            var text = client.DownloadString("https://www.saltybet.com/shaker?bracket=1");
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(text));
            var tournamentTypeRaw = document.QuerySelectorAll("div#compendiumright strong").FirstOrDefault().TextContent;
            var type = Regex.Match(tournamentTypeRaw, @"(\w+(?:\s\w+)?) Tournament Bracket").Groups[1].Value;
            var title = String.Equals(type, "Custom") ? document.QuerySelectorAll("div#compendiumright span.goldtext").FirstOrDefault().TextContent : "";
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
    }
}
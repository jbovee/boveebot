using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
                        var chnl = _discord.GetChannel(channelId) as IMessageChannel;
                        await chnl.SendMessageAsync(String.Format("{0} Tourney starting soon!", saltyRole));
                    }
                    break;
            }
        }
    }
}
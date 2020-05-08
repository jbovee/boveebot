using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
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
        // List<User> users;

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

            //_discord.MessageReceived += OnMessageReceivedAsync;
        }
    }
}
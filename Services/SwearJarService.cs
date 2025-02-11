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
    public class SwearJarService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        public SwearJarService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        public bool IsCommandOrBot(SocketUserMessage msg, string prefix, out int argPos)
        {
            argPos = 0;
            if ((msg.Author.Id == _discord.CurrentUser.Id) || msg.Author.IsBot) return true;
            bool hasStringPrefix = prefix == null ? false : msg.HasStringPrefix(prefix, ref argPos);
            return (hasStringPrefix || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos));
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            string prefix = _config["prefix"];

            if (!IsCommandOrBot(msg, prefix, out int argPos))
            {
                User usr = Users.GetOrCreateUser(msg.Author);
                List<string> allswears = DataStorage.GetAllSwears();
                if ((allswears == null) || (allswears.Count() == 0)) return;
                string rxstring = "(?:\\b|^)(?<swear>" + string.Join("|", allswears) + ")(?:\\s|$)";
                Regex rx = new Regex(rxstring, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var swearlist = rx.Matches(msg.Content)
                    .OfType<Match>()
                    .Select (m => m.Groups[1].Value)
                    .OrderBy(n => n);
                
                var swearlistdistinct = swearlist.Distinct();
                int len = swearlistdistinct.Count();
                string said = "";
                if (len < 1) return;
                foreach (var swear in swearlist)
                {
                    Users.AddOrIncrementUsed(usr, swear);
                    Users.IncrementOwed(usr);
                }
                if (len == 1) said = swearlistdistinct.FirstOrDefault();
                else if (len == 2) said = string.Join(" and ", swearlistdistinct);
                else said = string.Join(", ", swearlistdistinct.Take(len - 1)) + ", and " + swearlistdistinct.LastOrDefault();

                await msg.Channel.SendMessageAsync($"{msg.Author.Username}, how could you say {said}!");
            }
        }
    }
}
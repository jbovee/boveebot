using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
            if (msg.Author.Id == _discord.CurrentUser.Id) return true; // Ignore self
            bool hasStringPrefix = prefix == null ? false : msg.HasStringPrefix(prefix, ref argPos);
            return (hasStringPrefix || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos));
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;

            string prefix = _config["prefix"];
            
            if (!IsCommandOrBot(msg, prefix, out int argPos))
            {
                List<string> allswears = DataStorage.GetAllSwears();
                if ((allswears == null) || (allswears.Count() == 0)) return;
                string rxstring = "(?:\\b|^)(?<swear>" + string.Join("|", allswears) + ")(?:\\s|$)";
                Regex rx = new Regex(rxstring, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var swearlist = rx.Matches(msg.Content)
                    .OfType<Match>()
                    .Select (m => m.Groups[1].Value)
                    .Distinct()
                    .OrderBy(n => n);
                
                int len = swearlist.Count();
                string said = "";
                if (len < 1) return;
                if (len == 1) said = swearlist.FirstOrDefault();
                else if (len == 2) said = string.Join(" and ", swearlist);
                else said = string.Join(", ", swearlist.Take(len - 1)) + ", and " + swearlist.LastOrDefault();

                await msg.Channel.SendMessageAsync($"{msg.Author.Username}, how could you say {said}!");
            }
        }
    }
}
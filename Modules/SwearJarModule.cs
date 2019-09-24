using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BoveeBot.Modules
{
    [Group("swearjar"), Alias("sj"), Name("SwearJar")]
    public class SwearJar : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        public SwearJar(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;

            _discord.MessageReceived += MessageReceivedAsync;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;     // Ensure the message is from a user/bot
            if (msg == null) return;
            if (msg.Author.Id == _discord.CurrentUser.Id) return;     // Ignore self when checking commands

            List<string> allswears = DataStorage.GetAllSwears();
            string rxstring = @"\b(?<swear>" + string.Join("|", allswears) + @")(?:s|es|ed|er|ing|ting)?\b";
            Regex rx = new Regex(rxstring, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var swearlist = rx.Matches(msg.Content)
                .OfType<Match>()
                .Select (m => m.Groups[1].Value)
                .Distinct()
                .OrderBy(s => s);
            
            int len = swearlist.Count();
            string said = "";
            if (len < 1) return;
            if (len == 1) said = swearlist.FirstOrDefault();
            else if (len == 2) said = string.Join(" and ", swearlist);
            else said = string.Join(", ", swearlist.Take(len - 1)) + ", and " + swearlist.LastOrDefault();
            await msg.Channel.SendMessageAsync($"{msg.Author.Username}, how could you say {said}!");
        }

        [Command("-add")]
        [Summary("Add a swear to be recognized by the bot")]
        public async Task AddSwearAsync(string swear)
        {
            if (swear.Length < 3 || swear.Length > 12)
            {
                await ReplyAsync("Swears must be between three and twelve characters long");
                return;
            }
            if (DataStorage.AddSwear(swear.ToLower()))
            {
                await ReplyAsync($"{swear} is now a bad word");
            } else {
                await ReplyAsync($"{swear} is already a bad word");
            }
        }

        [Command("-del")]
        [Summary("Remove a swear from the list of recognized swears")]
        public async Task DelSwearAsync(string swear)
        {
            if (DataStorage.DelSwear(swear.ToLower()))
            {
                await ReplyAsync($"{swear} is no longer a bad word");
            } else {
                await ReplyAsync($"{swear} is not currently a bad word");
            }
        }

        [Command("-list")]
        [Alias("-ls")]
        [Summary("List all currently recognized swears")]
        public async Task ListSwearsAsync()
        {
            List<string> allswears = DataStorage.GetAllSwears();
            if ((allswears == null) || (allswears.Count == 0))
            {
                await ReplyAsync("There are currently no recognized swears");
                return;
            }
            string prefix = _config["prefix"];
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
            };
            builder.AddField(x => {
                x.Name = "Recognized swears";
                x.Value = string.Join("\n", allswears);
                x.IsInline = false;
            });
            await ReplyAsync("", false, builder.Build());
        }
    }
}

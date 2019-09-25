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
        private static string validateMsg = "does not match format requirements:\n\t- Between 3 and 12 characters\n\t- Using only letters with a single hyphen or apostrophe within them";

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

        }

        private bool IsValid(string swear)
        {
            bool validadd = new Regex(@"^([^'-][\w]+((-|')[\w]+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(swear).Success;
            if (swear.Length < 3 || swear.Length > 12 || !validadd) return false;
            return true;
        }

        [Command("-add")]
        [Summary("Add a swear to be recognized by the bot")]
        public async Task AddSwearAsync(string swear)
        {
            if (!IsValid(swear))
            {
                await ReplyAsync($"{swear} {validateMsg}");
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

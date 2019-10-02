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

        [Command]
        public async Task ShowSwearjar()
        {
            List<User> users = Users.GetAllUsers();
            uint max = 0; uint min = uint.MaxValue;
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
            };
            for (int u = 0; u < users.Count(); u++)
            {
                users[u].Username = _discord.GetUser(users[u].Id).Username;
                if (users[u].Owed > max) max = users[u].Owed;
                if (users[u].Owed < min) min = users[u].Owed;
            }
            users = users.OrderBy(user => user.Username).ToList();
            if (min == uint.MaxValue) min = 0;
            foreach (var user in users)
            {
                builder.AddField(x => {
                    x.Name = string.Format("{0} - {1}", user.Username, user.Owed == max ? $"**${user.Owed}**" : user.Owed == min ? $"*${user.Owed}*" : $"${user.Owed}");
                    x.Value = $"{string.Join(", ", user.Used.OrderByDescending(swear => swear.Value).Select(u => String.Format("{0}: {1}", u.Key, u.Value)))}";
                    x.IsInline = false;
                });
            }
            
            await ReplyAsync("", false, builder.Build());
        }

        private bool IsValid(string swear)
        {
            bool validadd = new Regex(@"^([\w]+((-|')[\w]+)?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(swear).Success;
            if (swear.Length < 3 || swear.Length > 12 || !validadd) return false;
            return true;
        }

        [Command("-add")]
        [Alias("-a")]
        [Summary("Add one to five swears to the bot")]
        public async Task AddSwearBatchAsync([Summary("<string>{1,5}")]params string[] swears)
        {
            if (swears.Length > 5)
            {
                await ReplyAsync("You may only add up to five words at a time");
                return;
            }
            foreach (var swear in swears)
            {
                if (!IsValid(swear)) await ReplyAsync($"{swear} {validateMsg}");
                else
                {
                    if (DataStorage.AddSwear(swear.ToLower()))
                    {
                        await ReplyAsync($"{swear} is now a bad word");
                    } else {
                        await ReplyAsync($"{swear} is already a bad word");
                    }
                }
            }
        }

        [Command("-del")]
        [Alias("-d")]
        [Summary("Delete one to five swears from the bot")]
        public async Task DelSwearBatchAsync([Summary("<string>{1,5}")]params string[] swears)
        {
            if (swears.Length > 5)
            {
                await ReplyAsync("You may only delete up to five words at a time");
                return;
            }
            foreach (var swear in swears)
            {
                if (DataStorage.DelSwear(swear.ToLower()))
                {
                    await ReplyAsync($"{swear} is no longer a bad word");
                } else {
                    await ReplyAsync($"{swear} is not currently a bad word");
                }
            }
        }

        [Command("-owed")]
        [Alias("-o")]
        [Summary("Show how much the user owes")]
        public async Task GetUserOwed()
        {
            User sender = Users.GetOrCreateUser(Context.User);
            await ReplyAsync($"{Context.User.Username}, you owe ${sender.Owed} to the swear jar");
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
            var builder = new EmbedBuilder();
            builder.AddField("Recognized swears",
                string.Join("\n", allswears))
                .WithColor(new Color(114, 137, 218));

            await ReplyAsync(embed: builder.Build());
        }

        [Command("-used")]
        [Alias("-u")]
        [Summary("Show how many times a user has used each swear")]
        public async Task GetUserUsed()
        {
            User sender = Users.GetOrCreateUser(Context.User);
            string swears = "";
            foreach (var swear in sender.Used)
            {
                swears += $"{swear.Key}: {swear.Value}\n";
            }

            var builder = new EmbedBuilder();
            builder.AddField("Your naughty vocabulary",
                swears)
                .WithColor(new Color(114, 137, 218));

            await ReplyAsync(embed: builder.Build());
        }
    }
}

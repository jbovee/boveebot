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
    [Group("group"), Alias("g"), Name("Group")]
    public class Group : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;
        private static string validateMsg = "does not match format requirements:\n\t- Between 3 and 12 characters\n\t- Using only letters with a single hyphen or apostrophe within them";

        public Group(
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
        
        /*
        {
            "Id": uint,
            "Name": string,
            "members": List<User> or List<uint> or List<SocketUser>,
        }
        */

        [Command("-create")]
        [Alias("-c")]
        [Summary("Create a new group")]
        public async Task CreateGroup(string groupname)
        {
            await ReplyAsync($"{groupname} successfully created");
        }
    }
}

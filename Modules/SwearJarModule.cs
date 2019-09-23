using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BoveeBot.Modules
{
    [Group("swearjar"), Name("SwearJar")]
    public class SwearJar : ModuleBase<SocketCommandContext>
    {
        [Command("-add")]
        [Summary("Add a swear to be recognized by the bot")]
        public async Task AddSwearAsync(string swear)
        {
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
    }
}

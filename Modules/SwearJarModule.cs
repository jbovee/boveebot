using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace BoveeBot.Modules
{
    [Group("swearjar"), Name("SwearJar")]
    public class SwearJar : ModuleBase<SocketCommandContext>
    {
        [Command("add")]
        [Summary("Add a swear to be recognized by the bot")]
        public async Task AddSwearAsync(string swear)
        {
            DataStorage.AddSwear(swear.ToLower());
            await ReplyAsync($"{swear} is now a bad word");
        }
    }
}

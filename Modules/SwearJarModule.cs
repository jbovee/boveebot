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
        private readonly IConfigurationRoot _config;

        public SwearJar(CommandService service, IConfigurationRoot config)
        {
            _config = config;
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
            string prefix = _config["prefix"];
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
            };
            builder.AddField(x => {
                x.Name = "Recognized swears";
                x.Value = string.Join("\n", DataStorage.GetAllSwears());
                x.IsInline = false;
            });
            await ReplyAsync("", false, builder.Build());
        }
    }
}

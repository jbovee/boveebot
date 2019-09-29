using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BoveeBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public HelpModule(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        private string GetAliases(IEnumerable<string> commands)
        {
            // swearjar -add, swearjar -a, sj -add, sj -a
            // swearjar -list, swearjar -ls, swearjar l, sj -list, sj -ls, sj l
            string prefix = _config["prefix"];
            string subfix = _config["subfix"];
            int cmdlen = commands.FirstOrDefault().Split(" ").Length;
            List<HashSet<string>> results = new List<HashSet<string>>();
            for (int i = 0; i < cmdlen; i++) results.Add(new HashSet<string>());
            foreach (var cmd in commands)
            {
                var split = cmd.Split(" ");
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].Substring(0,1) == subfix) results[i].Add(split[i].Substring(1));
                    else results[i].Add(split[i]);
                }
            }
            // ~(swearjar|sj) -(add|a)
            List<string> result = new List<string>();
            for (int j = 0; j < results.Count; j++)
            {
                if (j == 0) result.Add(results[j].Count > 1 ? $"\\{prefix}({string.Join("|", results[j])})" : $"\\{prefix}{results[j].FirstOrDefault()}");
                else result.Add(results[j].Count > 1 ? $"\\{subfix}({string.Join("|", results[j])})" : $"\\{subfix}{results[j].FirstOrDefault()}");
            }
            return string.Join(" ", result);
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };
            
            foreach (var module in _service.Modules)
            {
                List<string> description = new List<string>();
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess) description.Add($"{GetAliases(cmd.Aliases)}");
                    foreach (var param in cmd.Parameters)
                    {
                        description.Add(param.Summary);
                    }
                    description.Add("\n");
                }
                string output = string.Join(" ", description);
                
                if (!string.IsNullOrWhiteSpace(output))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = output;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            string prefix = _config["prefix"];
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + 
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}

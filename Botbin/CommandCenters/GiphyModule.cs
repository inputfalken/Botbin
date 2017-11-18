using System;
using System.Threading.Tasks;
using Botbin.Services.Interfaces;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin.CommandCenters {
    public class GifModule : ModuleBase<SocketCommandContext> {
        private readonly IGifProvider _giphy;

        public GifModule(IServiceProvider provider) => _giphy = provider.GetService<IGifProvider>();

        [Command("wtf", RunMode = RunMode.Async)]
        [Summary("Display a random GIF based on a search term")]
        public async Task Wtf([Remainder] string message) {
            try {
                var link = (await _giphy.Term(message)).AbsoluteUri;
                await Context.Channel.SendMessageAsync(link);
            }
            catch (Exception e) {
                //await Program.Log(new LogMessage(LogSeverity.Error, "Method WtfAsync",
                //    "Failed to deserialize an anonymous type", e));
                var errorResponse =
                    $"Oops, could not find a GIF for '{message}', try again with a different string.";
                await Context.Channel.SendMessageAsync(errorResponse);
            }
        }

        [Command("wtf", RunMode = RunMode.Async)]
        [Summary("Display a random GIF")]
        public async Task Wtf() {
            var link = (await _giphy.Random()).AbsoluteUri;

            await Context.Channel.SendMessageAsync(link);
        }
    }
}
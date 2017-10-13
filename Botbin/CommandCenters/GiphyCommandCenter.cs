using System;
using System.Threading.Tasks;
using Botbin.Giphy;
using Discord;
using Discord.Commands;

namespace Botbin.CommandCenters {
    public class GiphyCommandCenter : ModuleBase<SocketCommandContext> {
        private static readonly string GiphyKey =
            Environment.GetEnvironmentVariable("GIPHY_API_KEY", EnvironmentVariableTarget.Machine);

        private readonly GiphyService _giphyService = new GiphyService(GiphyKey);

        [Command("giphy", RunMode = RunMode.Async)]
        [Summary("Display a random GIF based on a search term")]
        public async Task Giphy([Remainder] string message) {
            try {
                var link = (await _giphyService.Search(message)).AbsoluteUri;
                await Context.Channel.SendMessageAsync(link);
            }
            catch (Exception e) {
                await Program.Log(new LogMessage(LogSeverity.Error, "Method WtfAsync",
                    "Failed to deserialize an anonymous type", e));
                var errorResponse =
                    $"Oops, could not find a GIF for '{message}', try again with a different string.";
                await Context.Channel.SendMessageAsync(errorResponse);
            }
        }

        [Command("giphy", RunMode = RunMode.Async)]
        [Summary("Display a random GIF")]
        public async Task Giphy() {
            var link = (await _giphyService.Random()).AbsoluteUri;

            await Context.Channel.SendMessageAsync(link);
        }
    }
}
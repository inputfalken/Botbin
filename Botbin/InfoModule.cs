using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Botbin {
    public class InfoModule : ModuleBase<SocketCommandContext> {
        private static readonly string GiphyKey =
            Environment.GetEnvironmentVariable("GIPHY_API_KEY", EnvironmentVariableTarget.Machine);

        private readonly Giphy _giphy = new Giphy(GiphyKey);

        [Command("say")]
        [Summary("Echos a message.")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo) =>
            await Context.Channel.SendMessageAsync(echo);

        [Command("days")]
        [Summary("Displays how many days the account has existed.")]
        public async Task DaysAsync() {
            var daysOfExistance = Math.Abs((Context.User.CreatedAt - DateTimeOffset.Now).Days);

            await Context.Channel.SendMessageAsync(
                $"Your account is {daysOfExistance} {(daysOfExistance == 1 ? "day" : "days")} old.");
        }

        [Command("wtf")]
        [Summary("Display a random GIF based on a search term")]
        public async Task WtfAsync([Remainder] string message) {
            try {
                var link = (await _giphy.Search(message)).AbsoluteUri;
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

        [Command("wtf")]
        [Summary("Display a random GIF")]
        public async Task WtfAsync() {
            var link = (await _giphy.Random()).AbsoluteUri;

            await Context.Channel.SendMessageAsync(link);
        }
    }
}
***REMOVED***
***REMOVED***
***REMOVED***
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

***REMOVED***
    public class InfoModule : ModuleBase<SocketCommandContext> {
        private static readonly string GiphyKey =
            Environment.GetEnvironmentVariable("GIPHY_API_KEY", EnvironmentVariableTarget.Machine);

***REMOVED***

        [Command("say")]
        [Summary("Echos a message.")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo) =>
            await Context.Channel.SendMessageAsync(echo);

        [Command("days")]
        [Summary("Displays how many days the account has existed.")]
        public async Task DaysAsync() {
            var daysOfExistance = Math.Abs((Context.User.CreatedAt - DateTimeOffset.Now).Days);

            await Context.Channel.SendMessageAsync(
                $"Your account is {daysOfExistance***REMOVED*** {(daysOfExistance == 1 ? "day" : "days")***REMOVED*** old.");
    ***REMOVED***

        [Command("wtf")]
        [Summary("Display a random GIF based on the message")]
        public async Task WtfAsync([Remainder] string message) {
            var randomGifRequest =
                $"https://api.giphy.com/v1/gifs/random?api_key={GiphyKey***REMOVED***&tag={message***REMOVED***&rating=R";
            var response = await _client.GetStringAsync(randomGifRequest);

            try {
                await Context.Channel.SendMessageAsync(GetGiphyUrlFromResponse(response));
        ***REMOVED***
            catch (Exception e) {
                await Program.Log(new LogMessage(LogSeverity.Error, "Method WtfAsync",
                    "Failed to deserialize an anonymous type", e));
                var errorResponse =
                    $"Oops, could not find a GIF for '{message***REMOVED***', try again with a different string.";
                await Context.Channel.SendMessageAsync(errorResponse);
        ***REMOVED***
    ***REMOVED***

        [Command("wtf")]
        [Summary("Display a random GIF")]
        public async Task WtfAsync() {
            var randomEndPoint =
                $"https://api.giphy.com/v1/gifs/random?api_key={GiphyKey***REMOVED***&tag=&rating=R";
            var response = await _client.GetStringAsync(randomEndPoint);

            await Context.Channel.SendMessageAsync(GetGiphyUrlFromResponse(response));
    ***REMOVED***

        private static string GetGiphyUrlFromResponse(string response) =>
            JsonConvert.DeserializeAnonymousType(response, new {data = new {url = string.Empty***REMOVED******REMOVED***).data.url;
***REMOVED***
***REMOVED***
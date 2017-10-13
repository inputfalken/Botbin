using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Botbin {
    public class UserEventsCommandCenter : ModuleBase<SocketCommandContext> {
        [Command("history", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task UserInfoAsync([Summary("The (optional) user to get info for")] SocketUser user = null) {
            var userInfo = user ?? Context.Client.CurrentUser;

            if (Program.UserEvents.TryGetValue(userInfo.Id, out var events)) {
                foreach (var userEvent in events) {
                    if (userEvent.Type == UserEventType.StartGame) {
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' started '{userEvent.Game}' at '{userEvent.Time}'.");
                    }
                    else {
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' quit '{userEvent.Game}' at {userEvent.Time}.");
                    }
                }
            }
            else {
                await Context.Channel.SendMessageAsync(
                    $"No game history found for user '{userInfo.Username}#{userInfo.Discriminator}'");
            }
        }
    }
}
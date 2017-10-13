using System.Linq;
using System.Threading.Tasks;
using Botbin.GameTracking;
using Botbin.GameTracking.UserEvent.Enums;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin.CommandCenters {
    public class UserEventsCommandCenter : ModuleBase<SocketCommandContext> {
        [Command("history", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task History([Summary("The (optional) user to get info for")] SocketUser user = null) {
            var userInfo = user ?? Context.Client.CurrentUser;
            var userEvents = await Program.Services
                .GetService<IUserEventRetriever>()
                .UserEventsById(userInfo.Id)
                .ToAsyncEnumerable()
                .ToArray();

            if (userEvents.Length > 0)
                foreach (var userEvent in userEvents)
                    if (userEvent.Action == UserAction.StartGame)
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' started '{userEvent.Game}' at '{userEvent.Time}'.");
                    else
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' quit '{userEvent.Game}' at {userEvent.Time}.");
            else
                await Context.Channel.SendMessageAsync(
                    $"No game history found for user '{userInfo.Username}#{userInfo.Discriminator}'");
        }
    }
}
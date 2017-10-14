using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking;
using Botbin.UserTracking.UserEvent.Enums;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Botbin.CommandCenters {
    public class UserEventsCommandCenter : ModuleBase<SocketCommandContext> {
        private readonly IUserEventRetriever _eventRetriever;

        public UserEventsCommandCenter() => _eventRetriever = Program.Services.GetService<IUserEventRetriever>();

        [Command("history", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task History([Summary("The (optional) user to get info for")] SocketUser user = null) {
            var userInfo = user ?? Context.Client.CurrentUser;
            var userEvents = await _eventRetriever
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

        [Command("save", RunMode = RunMode.Async)]
        public async Task Save() {
            const ulong admin = 318468838058360846;
            if (Context.User.Id == admin) {
                var userEvents = await _eventRetriever
                    .UserEvents()
                    .ToAsyncEnumerable()
                    .ToArray();
                if (userEvents.Length > 0) {
                    await File.WriteAllTextAsync(".\\history.json", JsonConvert.SerializeObject(userEvents));
                    await Context.Channel.SendMessageAsync("Successfully saved content to disc.");
                }
                else {
                    await Context.Channel.SendMessageAsync("No data available to save.");
                }
            }
            else {
                await Context.Channel.SendMessageAsync("Pfft, i wont save this. You are not the boss of me!");
            }
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity() {
            await _eventRetriever
                .UserEvents()
                .ToAsyncEnumerable()
                .ForEachAsync(async user => {
                    var response = $"{user.Username} {user.Action} at {user.Time}";
                    await Context.Channel.SendMessageAsync(response);
                });
        }
    }
}
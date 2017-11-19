using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.Services.UserTracking;
using Botbin.Services.UserTracking.UserEvent;
using Botbin.Services.UserTracking.UserEvent.Implementations;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Optional;
using static System.Environment;

namespace Botbin.CommandCenters {
    public class UserEventModule : ModuleBase<SocketCommandContext> {
        private readonly IUserEventRetriever _eventRetriever;

        public UserEventModule(IServiceProvider provider) {
            _eventRetriever = provider.GetService<IUserEventRetriever>();
            provider.GetService<Settings>();
        }

        private static string Activities(IEnumerable<IUserEvent> events, string header = "Activities")
            => events
                .OrderBy(e => e.Occurrence)
                .Aggregate(
                    $"__**{header}**__:{NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} at {c.Occurrence}{NewLine}"
                    , s => $"{s}```"
                );

        private static string GameHistory(IEnumerable<UserGame> events, string header = "Game History")
            => events
                .OrderBy(e => e.Occurrence)
                .Aggregate(
                    $"__**{header}**__:{NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} {c.Game} at {c.Occurrence}{NewLine}"
                    , s => $"{s}```"
                );

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task GameHistory([Summary("The (optional) user to get info for")] SocketUser user) {
            var result = _eventRetriever
                .UserEventsById(user.Id)
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .ToList()
                .SomeWhen(l => l.Any())
                .Match(l =>
                        GameHistory(l, $"Game History for '{user.Username}'"),
                    () => $"No game history found for user '{user.Username}'"
                );
            await ReplyAsync(result);
        }

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history for everyone.")]
        public async Task GameHistory() {
            var result = _eventRetriever
                .UserEvents()
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .ToList()
                .SomeWhen(l => l.Any())
                .Match(l =>
                        GameHistory(l),
                    () => "No game history found."
                );

            await ReplyAsync(result);
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity() {
            var result = _eventRetriever
                .UserEvents()
                .ToList()
                .SomeWhen(l => l.Any())
                .Match(l =>
                        Activities(l),
                    () => "Could not find any activity."
                );
            await ReplyAsync(result);
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity([Summary("The user to get activity info for")] SocketUser user) {
            var result = _eventRetriever
                .UserEventsById(user.Id)
                .ToList()
                .SomeWhen(l => l.Any())
                .Match(l =>
                        Activities(l, $"Activities for '{user.Username}'"),
                    () => $"Could not find activity for '{user.Username}'."
                );

            await ReplyAsync(result);
        }
    }
}
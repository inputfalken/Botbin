using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Implementations;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
                .OrderBy(e => e.Time)
                .Aggregate(
                    $"__**{header}**__:{NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} at {c.Time}{NewLine}"
                    , s => $"{s}```"
                );

        private static string GameHistory(IEnumerable<UserGame> events, string header = "Game History")
            => events
                .OrderBy(e => e.Time)
                .Aggregate(
                    $"__**{header}**__:{NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} {c.Game} at {c.Time}{NewLine}"
                    , s => $"{s}```"
                );

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task GameHistory([Summary("The (optional) user to get info for")] SocketUser user) {
            var userGames = _eventRetriever
                .UserEventsById(user.Id)
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .OrderBy(game => game.Time)
                .ToList();

            if (userGames.Any())
                await ReplyAsync(GameHistory(userGames, $"Game History for '{user.Username}'"));
            else
                await ReplyAsync($"No game history found for user '{user.Username}'");
        }

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history for everyone.")]
        public async Task GameHistory() {
            var userGames = _eventRetriever
                .UserEvents()
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .OrderBy(game => game.Time)
                .ToList();

            if (userGames.Any()) await ReplyAsync(GameHistory(userGames));
            else await ReplyAsync("No game history found.");
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity() {
            var events = _eventRetriever
                .UserEvents()
                .ToList();
            if (events.Any()) await ReplyAsync(Activities(events));
            else await ReplyAsync("Could not find any activity.");
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity([Summary("The user to get activity info for")] SocketUser user) {
            var events = _eventRetriever
                .UserEventsById(user.Id)
                .ToList();
            if (events.Any())
                await ReplyAsync(Activities(events, $"Activities for '{user.Username}'"));
            else await ReplyAsync($"Could not find activity for '{user.Username}'.");
        }
    }
}
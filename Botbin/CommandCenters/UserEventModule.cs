﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Botbin.UserTracking;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Implementations;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Botbin.CommandCenters {
    public class UserEventModule : ModuleBase<SocketCommandContext> {
        private readonly IUserEventRetriever _eventRetriever;
        private readonly Settings _settings;

        public UserEventModule(IServiceProvider provider) {
            _eventRetriever = provider.GetService<IUserEventRetriever>();
            _settings = provider.GetService<Settings>();
        }

        private static string FormatActivities(IEnumerable<IUserEvent> events, string header = "Activities")
            => events
                .OrderBy(e => e.Time)
                .Aggregate(
                    $"__**{header}**__:{Environment.NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} at {c.Time}{Environment.NewLine}"
                    , s => $"{s}```"
                );

        private static string FormatGameHistory(IEnumerable<UserGame> events, string header = "Game History")
            => events
                .OrderBy(e => e.Time)
                .Aggregate(
                    $"__**{header}**__:{Environment.NewLine}```",
                    (a, c) => $"{a}{c.Username} {c.Action} {c.Game} at {c.Time}{Environment.NewLine}"
                    , s => $"{s}```"
                );

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task GameHistory([Summary("The (optional) user to get info for")] SocketUser user) {
            var userGames = _eventRetriever
                .UserEventsById(user.Id)
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .OrderBy(game => game.Time);

            if (userGames.Any())
                await Context.Channel.SendMessageAsync(FormatGameHistory(
                    userGames,
                    $"Game History for '{user.Username}'")
                );
            else
                await Context.Channel.SendMessageAsync(
                    $"No game history found for user '{user.Username}'"
                );
        }

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history for everyone.")]
        public async Task GameHistory() {
            var userGames = _eventRetriever
                .UserEvents()
                .Where(u => u is UserGame)
                .Cast<UserGame>()
                .OrderBy(game => game.Time);

            if (userGames.Any()) await Context.Channel.SendMessageAsync(FormatGameHistory(userGames));
            else await Context.Channel.SendMessageAsync("No game history found.");
        }

        [Command("save", RunMode = RunMode.Async)]
        public async Task Save() {
            if (_settings.IsAdmin(Context.User)) {
                var userEvents = _eventRetriever.UserEvents().ToList();
                using (var client = new TcpClient()) {
                    client.Connect(IPAddress.Parse("192.168.99.100"), 5000);
                    Thread.Sleep(200);
                    var line = JsonConvert.SerializeObject(userEvents) + "\n";
                    using (var streamWriter = new StreamWriter(client.GetStream())) {
                        streamWriter.Write(line);
                    }
                }
                if (userEvents.Any()) {
                    //await File.WriteAllTextAsync(".\\history.json", JsonConvert.SerializeObject(userEvents));
                    //await Context.Channel.SendMessageAsync("Successfully saved content to disc.");
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
            var events = _eventRetriever.UserEvents();
            if (events.Any()) await Context.Channel.SendMessageAsync(FormatActivities(events));
            else await Context.Channel.SendMessageAsync("Could not find any activity.");
        }

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity([Summary("The user to get activity info for")] SocketUser user) {
            var events = _eventRetriever.UserEventsById(user.Id);
            if (events.Any())
                await Context.Channel.SendMessageAsync(FormatActivities(events, $"Activities for '{user.Username}'"));
            else await Context.Channel.SendMessageAsync($"Could not find activity for '{user.Username}'.");
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Enums;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static System.Environment;

namespace Botbin.CommandCenters {
    public class UserEventModule : ModuleBase<SocketCommandContext> {
        private readonly IUserEventRetriever _eventRetriever;

        public UserEventModule(IServiceProvider provider) =>
            _eventRetriever = provider.GetService<IUserEventRetriever>();

        [Command("gamehistory", RunMode = RunMode.Async)]
        [Summary("Retrieves the  game history of the user.")]
        public async Task GameHistory([Summary("The (optional) user to get info for")] SocketUser user = null) {
            var userInfo = user ?? Context.Client.CurrentUser;
            var userEvents = await _eventRetriever
                .UserEventsById(userInfo.Id)
                .Where(u => u.Action == UserAction.QuitGame || u.Action == UserAction.StartGame)
                .ToAsyncEnumerable()
                .ToArray();

            if (userEvents.Length > 0) {
                foreach (var userEvent in userEvents)
                    if (userEvent.Action == UserAction.StartGame)
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' started '{userEvent.Game}' at '{userEvent.Time}'.");
                    else if (userEvent.Action == UserAction.QuitGame)
                        await Context.Channel.SendMessageAsync(
                            $"User '{userEvent.Username}' quit '{userEvent.Game}' at {userEvent.Time}.");
            }
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
            var events = await _eventRetriever
                .UserEvents()
                .ToAsyncEnumerable()
                .ToList();
            if (events.Any()) {
                await Context.Channel.SendMessageAsync(FormatActivities(events));
            }
            else await Context.Channel.SendMessageAsync("Could not find any activity.");
        }

        private static string FormatActivities(IEnumerable<IUserEvent> events) => events.Aggregate(
            $"__**Activities**__:{NewLine}```",
            (a, c) => $"{a}{c.Username} {c.Action.ToString()} at {c.Time}{NewLine}"
            , s => $"{s}```"
        );

        [Command("activity", RunMode = RunMode.Async)]
        public async Task Activity([Summary("The user to get info for")] SocketUser user) {
            var events = await _eventRetriever.UserEventsById(user.Id)
                .ToAsyncEnumerable()
                .ToList();
            if (events.Any()) await Context.Channel.SendMessageAsync(FormatActivities(events));
            else await Context.Channel.SendMessageAsync($"Could not find activity for '{user.Username}'.");
        }
    }
}
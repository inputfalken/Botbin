﻿using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Botbin.CommandCenters {
    public class InfoCommandModule : ModuleBase<SocketCommandContext> {
        public InfoCommandModule(IServiceProvider provider) { }

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
    }
}
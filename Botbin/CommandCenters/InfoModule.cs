using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;

namespace Botbin.CommandCenters {
    public class InfoModule : ModuleBase<SocketCommandContext> {
        public InfoModule(IServiceProvider provider) { }

        [Command("days")]
        [Summary("Displays how many days the account has existed.")]
        public async Task DaysAsync() {
            var daysOfExistance = Math.Abs((Context.User.CreatedAt - DateTimeOffset.Now).Days);

            await Context.Channel.SendMessageAsync(
                $"Your account is {daysOfExistance} {(daysOfExistance == 1 ? "day" : "days")} old.");
        }

        [Command("uptime")]
        public async Task Uptime() {
            await ReplyAsync((DateTime.Now - Process.GetCurrentProcess().StartTime).ToString("G"));
        }
    }
}
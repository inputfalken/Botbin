using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Botbin.CommandCenters {
    public class RandomizerModule : ModuleBase<SocketCommandContext> {
        private static readonly Random Randomizer = new Random();
        private const string Oops = "Oops, something went wrong";

        // Should be an optional<T>
        private static (T, bool) Randomize<T>(IReadOnlyList<T> items) => items.Any()
            ? (value: items[Randomizer.Next(items.Count)], hasValue: true)
            : (value:default(T), hasValue: false);

        [Command("rnd", RunMode = RunMode.Async)]
        public async Task RandomizeArgs([Remainder] string message) {
            (string value, bool hasValue) = Randomize(message.Split(null));
            await ReplyAsync(hasValue ? value : Oops);
        }
    }
}
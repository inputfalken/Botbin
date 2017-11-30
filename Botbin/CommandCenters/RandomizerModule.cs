using System;
using System.Threading.Tasks;
using Botbin.Services;
using Botbin.Services.Interfaces;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin.CommandCenters {
    public class RandomizerModule : ModuleBase<SocketCommandContext> {
        private readonly IRandomizer _randomizer;

        public RandomizerModule(IServiceProvider provider) => _randomizer = provider.GetService<IRandomizer>();

        private const string Oops = "Oops, something went wrong";

        [Command("random-item", RunMode = RunMode.Async)]
        public async Task RandomizeArgs([Remainder] string message) =>
            await ReplyAsync(_randomizer.Collection(message.Split(null)).ValueOr(Oops));

        [Command("random-number", RunMode = RunMode.Async)]
        public async Task RandomizeNumber(int max) =>
            await ReplyAsync(_randomizer.Integer(max).ToString());

        [Command("random-number", RunMode = RunMode.Async)]
        public async Task RandomizeNumber(int min, int max) =>
            await ReplyAsync(_randomizer.Integer(min, max).ToString());

        [Command("random-number", RunMode = RunMode.Async)]
        public async Task RandomizeNumber() =>
            await ReplyAsync(_randomizer.Integer(0, 101).ToString());
    }
}
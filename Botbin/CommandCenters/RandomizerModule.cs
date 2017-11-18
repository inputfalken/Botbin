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

        [Command("rnd", RunMode = RunMode.Async)]
        public async Task RandomizeArgs([Remainder] string message) =>
            await ReplyAsync(_randomizer.Collection(message.Split(null)).ValueOr(Oops));
    }
}
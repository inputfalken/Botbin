using System;
using System.Threading.Tasks;
using Botbin.CommandCenters;
using Botbin.Giphy;
using Botbin.UserTracking;
using Botbin.UserTracking.Implementations;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static System.Environment;
using static System.EnvironmentVariableTarget;

namespace Botbin {
    internal static class Program {
        private static readonly IServiceProvider Services = new ServiceCollection()
            .AddSingleton(p => new CommandService())
            .AddSingleton(p => new DiscordSocketClient())
            .AddSingleton<ILogger>(p => new JsonLinesTcpLogger(GetEnvironmentVariable("LOGSTASH_ADDRESS", Process), 5000, new ConsoleLogger()))
            .AddSingleton(p => new GiphyService(GetEnvironmentVariable("GIPHY_API_KEY", Process)))
            .AddSingleton(p =>
                new Settings('~', GetEnvironmentVariable("DISCORD_BOT_TOKEN", Process), 318468838058360846)
            )
            .AddSingleton(p => new ConcurrentInMemoryUserTracker(p))
            .AddSingleton<IUserListener>(p => p.GetService<ConcurrentInMemoryUserTracker>())
            .AddSingleton<IUserEventRetriever>(p => p.GetService<ConcurrentInMemoryUserTracker>())
            .BuildServiceProvider();

        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        private static async Task StartAsync() {
            var client = Services.GetService<DiscordSocketClient>();
            var listener = Services.GetService<IUserListener>();
            var settings = Services.GetService<Settings>();
            await AddModules(Services.GetService<CommandService>());

            client.Log += Log;
            client.GuildMemberUpdated += listener.ListenForGames;
            client.GuildMemberUpdated += listener.ListenForLoginsAndLogOuts;
            client.MessageReceived += HandleCommandAsync;
            client.MessageReceived += listener.ListenForMessages;

            await client.LoginAsync(TokenType.Bot, settings.BotToken);
            await client.StartAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private static async Task AddModules(CommandService commandService) {
            await commandService.AddModuleAsync<GiphyModule>();
            await commandService.AddModuleAsync<UserEventModule>();
            await commandService.AddModuleAsync<InfoModule>();
        }

        private static async Task HandleCommandAsync(SocketMessage messageParam) {
            var settings = Services.GetService<Settings>();
            var commands = Services.GetService<CommandService>();
            var client = Services.GetService<DiscordSocketClient>();
            // Don't process the command if it was a System Message
            if (!(messageParam is SocketUserMessage message)) return;
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix(settings.CommandPrefix, ref argPos) ||
                  message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new SocketCommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, Services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        public static Task Log(LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
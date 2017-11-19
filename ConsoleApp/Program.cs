using System;
using System.Threading.Tasks;
using Botbin;
using Botbin.CommandCenters;
using Botbin.Services;
using Botbin.Services.Interfaces;
using Botbin.Services.UserTracking;
using Botbin.Services.UserTracking.Implementations;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using static System.DateTime;
using static System.Environment;
using static System.EnvironmentVariableTarget;
using static System.UriKind;

namespace ConsoleApp {
    internal static class Program {
        private const ulong AdminId = 318468838058360846;
        private const char CommandPrefix = '~';
        private const int LogstashPort = 5000;
        private const int ElasticPort = 9200;

        private static readonly string TcpIp = GetEnvironmentVariable("LOGSTASH_ADDRESS", Process);
        private static readonly string GiphyApiKey = GetEnvironmentVariable("GIPHY_API_KEY", Process);
        private static readonly string BotToken = GetEnvironmentVariable("DISCORD_BOT_TOKEN", Process);
        private static readonly string ElasticsearchAddress = $"http://{TcpIp}:{ElasticPort}";

        private static readonly IServiceProvider Services = new ServiceCollection()
            .AddSingleton(p => new CommandService())
            .AddSingleton(p => new DiscordSocketClient())
            .AddSingleton<IRandomizer>(p => new Randomizer(new Random()))
            .AddSingleton<ILogger>(p => new JsonTcpLogger(TcpIp, LogstashPort, new ConsoleLogger()))
            .AddSingleton<IGifProvider>(p => new Giphy(GiphyApiKey))
            .AddSingleton(p => new Settings(CommandPrefix, BotToken, AdminId))
            .AddSingleton(p => new ConcurrentUserTracker(p))
            .AddSingleton<IUserListener>(p => p.GetService<ConcurrentUserTracker>())
            .AddSingleton<IUserEventRetriever>(p => new Elastic(new Uri(ElasticsearchAddress, Absolute), Today))
            .BuildServiceProvider();

        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        private static async Task StartAsync() {
            var client = Services.GetService<DiscordSocketClient>();
            var listener = Services.GetService<IUserListener>();
            var settings = Services.GetService<Settings>();
            await AddModules(Services.GetService<CommandService>());

            client.Log += Log;
            client.GuildMemberUpdated += listener.ListenForGames;
            client.GuildMemberUpdated += listener.ListenForStatus;
            client.MessageReceived += HandleCommandAsync;
            client.MessageReceived += listener.ListenForMessages;

            await client.LoginAsync(TokenType.Bot, settings.BotToken);
            await client.StartAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private static async Task AddModules(CommandService commandService) {
            await commandService.AddModuleAsync<GifModule>();
            await commandService.AddModuleAsync<UserEventModule>();
            await commandService.AddModuleAsync<InfoModule>();
            await commandService.AddModuleAsync<RandomizerModule>();
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
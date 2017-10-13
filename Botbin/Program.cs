using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin {
    internal static class Program {
        private static readonly IServiceProvider Services = new ServiceCollection()
            .AddSingleton(new CommandService())
            .AddSingleton(new DiscordSocketClient())
            .BuildServiceProvider();

        private static readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> Dictionary =
            new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();

        public static ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> UserEvents { get; } = Dictionary;

        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        private static async Task StartAsync() {
            var client = Services.GetService<DiscordSocketClient>();
            client.Log += Log;
            client.GuildMemberUpdated += Discord_UserUpdated;
            var commandService = Services.GetService<CommandService>();
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly());
            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private static Task Discord_UserUpdated(SocketUser before, SocketUser after) {
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            var id = before.Id;

            if (startGame) {
                Dictionary.AddOrUpdate(
                    id,
                    _ => Add(after, UserEventType.StartGame),
                    (_, queue) => Update(after, UserEventType.StartGame, queue)
                );
            }
            else if (quitGame) {
                Dictionary.AddOrUpdate(
                    id,
                    _ => Add(before, UserEventType.QuitGame),
                    (_, queue) => Update(before, UserEventType.QuitGame, queue)
                );
            }
            return Task.CompletedTask;
        }

        private static ConcurrentQueue<IUserEvent> Add(IUser user, UserEventType type) {
            var concurrentQueue = new ConcurrentQueue<IUserEvent>();
            concurrentQueue.Enqueue(new UserEvent(user, type));
            return concurrentQueue;
        }

        private static ConcurrentQueue<IUserEvent> Update(IUser user, UserEventType type,
            ConcurrentQueue<IUserEvent> events) {
            events.Enqueue(new UserEvent(user, type));
            return events;
        }

        private static async Task HandleCommandAsync(SocketMessage messageParam) {
            var commands = Services.GetService<CommandService>();
            var client = Services.GetService<DiscordSocketClient>();
            // Don't process the command if it was a System Message
            if (!(messageParam is SocketUserMessage message)) return;
            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) ||
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
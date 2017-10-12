﻿using System;
using System.Collections.Generic;
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

        private static readonly Dictionary<string, (DateTime, SocketUser)> Dictionary =
            new Dictionary<string, (DateTime, SocketUser)>();

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

        private static async Task Discord_UserUpdated(SocketUser before, SocketUser after) {
            if (!before.Game.HasValue && after.Game.HasValue) {
                Dictionary.Add(after.Username, (DateTime.Now, after));
                var message = $"User {after.Username} is playing {after.Game.Value.Name}";
                await Log(new LogMessage(LogSeverity.Info, "GuildMemberUpdated", message));
            }
            else if (before.Game.HasValue && !after.Game.HasValue) {
                var valueTuple = Dictionary[after.Username];
                var timeSpan = DateTime.Now - valueTuple.Item1;
                var message =
                    $"{valueTuple.Item2.Username} played {before.Game} for {timeSpan.Minutes} min and  {timeSpan.Seconds} sec.";
                await Log(new LogMessage(LogSeverity.Info, "GuildMemberUpdated", message));
            }
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
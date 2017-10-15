﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Botbin.UserTracking;
using Botbin.UserTracking.Implementations;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Botbin {
    internal static class Program {
        public static IServiceProvider Services { get; } = new ServiceCollection()
            .AddSingleton(new CommandService())
            .AddSingleton(new DiscordSocketClient())
            .AddSingleton<IUserTracker>(p => new ConcurrentInMemoryUserTracker())
            .AddSingleton<IUserListener>(p => p.GetService<IUserTracker>())
            .AddSingleton<IUserEventRetriever>(p => p.GetService<IUserTracker>())
            .BuildServiceProvider();

        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        private static async Task StartAsync() {
            var client = Services.GetService<DiscordSocketClient>();
            var listener = Services.GetService<IUserListener>();
            var commandService = Services.GetService<CommandService>();

            client.Log += Log;
            client.GuildMemberUpdated += listener.ListenForGames;
            client.GuildMemberUpdated += listener.ListenForLoginsAndLogOuts;
            client.MessageReceived += HandleCommandAsync;
            client.MessageReceived += listener.ListenForMessages;

            await commandService.AddModulesAsync(Assembly.GetEntryAssembly());
            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            // Block this task until the program is closed.
            await Task.Delay(-1);
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
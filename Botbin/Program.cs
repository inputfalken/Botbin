***REMOVED***
using System.Reflection;
***REMOVED***
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

***REMOVED***
    internal static class Program {
        private static readonly IServiceProvider Services = new ServiceCollection()
            .AddSingleton(new CommandService())
            .AddSingleton(new DiscordSocketClient())
            .BuildServiceProvider();

        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();

        private static async Task StartAsync() {
            var client = Services.GetService<DiscordSocketClient>();
            client.Log += Log;
            var commandService = Services.GetService<CommandService>();
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly());
            var token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine);
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
    ***REMOVED***

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
    ***REMOVED***

        public static Task Log(LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
    ***REMOVED***
***REMOVED***
***REMOVED***
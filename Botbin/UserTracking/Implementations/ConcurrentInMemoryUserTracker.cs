using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Implementations;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using static System.Threading.Tasks.Task;
using static Botbin.UserTracking.UserEvent.Enums.UserAction;
using static Discord.UserStatus;

namespace Botbin.UserTracking.Implementations {
    internal class ConcurrentInMemoryUserTracker : IUserListener, IUserEventRetriever {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;
        private readonly ILogger _logger;
        private readonly Settings _settings;

        public ConcurrentInMemoryUserTracker(IServiceProvider provider) {
            _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();
            _settings = provider.GetService<Settings>();
            _logger = provider.GetService<ILogger>();
        }

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task ListenForGames(IUser before, IUser after) {
            if (NotHuman(before)) return CompletedTask;
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            UserLog userLog;
            if (startGame) userLog = new UserGame(before, StartGame, after.Game.Value);
            else if (quitGame) userLog = new UserGame(after, QuitGame, before.Game.Value);
            else return CompletedTask;
            Save(userLog);
            _logger.Log(JsonConvert.SerializeObject(userLog));
            return CompletedTask;
        }

        public Task ListenForLoginsAndLogOuts(IUser before, IUser after) {
            if (NotHuman(before)) return CompletedTask;
            var logIn = before.Status == Offline && after.Status == Online;
            var logOff = before.Status == Online && after.Status == Offline;
            UserLog userLog;
            if (logIn) userLog = new UserLog(after, LogIn);
            else if (logOff) userLog = new UserLog(after, LogOff);
            else return CompletedTask;
            Save(userLog);
            _logger.Log(JsonConvert.SerializeObject(userLog));
            return CompletedTask;
        }

        public Task ListenForMessages(IMessage message) {
            var author = message.Author;
            if (NotHuman(author) || Command(message.Content)) return CompletedTask;
            var user = new UserMessage(author, SendMessage, message.Content);
            _logger.Log(JsonConvert.SerializeObject(user));

            _dictionary.AddOrUpdate(
                user.Id,
                _ => {
                    var concurrentQueue = new ConcurrentQueue<IUserEvent>();
                    concurrentQueue.Enqueue(user);
                    return concurrentQueue;
                },
                (_, queue) => {
                    queue.Enqueue(user);
                    return queue;
                }
            );
            return CompletedTask;
        }

        private bool Command(string message) => message.StartsWith(_settings.CommandPrefix);

        private static bool NotHuman(IUser user) => user.IsWebhook || user.IsBot;

        private void Save(IUserEvent user) =>
            _dictionary.AddOrUpdate(
                user.Id,
                _ => NewKey(user),
                (_, queue) => Update(user, queue)
            );

        private static ConcurrentQueue<IUserEvent> NewKey(IUserEvent user) =>
            Update(user, new ConcurrentQueue<IUserEvent>());

        private static ConcurrentQueue<IUserEvent> Update(IUserEvent user,
            ConcurrentQueue<IUserEvent> events) {
            events.Enqueue(user);
            return events;
        }
    }
}
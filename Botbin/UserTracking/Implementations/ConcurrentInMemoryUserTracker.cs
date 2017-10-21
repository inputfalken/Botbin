using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Implementations;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using static Botbin.UserTracking.UserEvent.Enums.UserAction;

namespace Botbin.UserTracking.Implementations {
    internal class ConcurrentInMemoryUserTracker : IUserListener, IUserEventRetriever {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;
        private readonly Settings _settings;

        public ConcurrentInMemoryUserTracker(IServiceProvider provider) {
            _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();
            _settings = provider.GetService<Settings>();
        }

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task ListenForGames(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            if (startGame) Save(new UserGame(before, StartGame, after.Game.Value));
            if (quitGame) Save(new UserGame(after, QuitGame, before.Game.Value));
            return Task.CompletedTask;
        }

        public Task ListenForLoginsAndLogOuts(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var logIn = before.Status == UserStatus.Offline && after.Status == UserStatus.Online;
            var logOff = before.Status == UserStatus.Online && after.Status == UserStatus.Offline;
            if (logIn) Save(new UserLog(after, LogIn));
            if (logOff) Save(new UserLog(after, LogOff));
            return Task.CompletedTask;
        }

        public Task ListenForMessages(IMessage message) {
            var author = message.Author;
            if (NotHuman(author) || Command(message.Content)) return Task.CompletedTask;
            var description = $"{SendMessage} '{message}'.";
            var user = new UserMessage(author, SendMessage, description);

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
            return Task.CompletedTask;
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
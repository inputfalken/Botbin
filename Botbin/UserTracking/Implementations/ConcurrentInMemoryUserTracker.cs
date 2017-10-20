using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.UserTracking.UserEvent;
using Botbin.UserTracking.UserEvent.Enums;
using Discord;
using static Botbin.UserTracking.UserEvent.Enums.UserAction;

namespace Botbin.UserTracking.Implementations {
    internal class ConcurrentInMemoryUserTracker : IUserListener, IUserEventRetriever {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;

        public ConcurrentInMemoryUserTracker() {
            _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();
        }

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task ListenForGames(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var id = before.Id;
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            if (startGame) Save(after, id, StartGame);
            if (quitGame) Save(before, id, QuitGame);
            return Task.CompletedTask;
        }

        public Task ListenForLoginsAndLogOuts(IUser before, IUser after) {
            if (NotHuman(before)) return Task.CompletedTask;
            var logIn = before.Status == UserStatus.Offline && after.Status == UserStatus.Online;
            var logOff = before.Status == UserStatus.Online && after.Status == UserStatus.Offline;
            var beforeId = before.Id;
            if (logIn) Save(after, beforeId, LogIn);
            if (logOff) Save(after, beforeId, LogOff);
            return Task.CompletedTask;
        }

        public Task ListenForMessages(IMessage message) {
            var author = message.Author;
            if (NotHuman(author)) return Task.CompletedTask;
            var description = $"{SendMessage} '{message}'.";
            var user = new UserEvent.Implementations.UserEvent(author, SendMessage, description);
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

        private static bool NotHuman(IUser user) => user.IsWebhook || user.IsBot;

        private void Save(IUser before, ulong id, UserAction action) =>
            _dictionary.AddOrUpdate(
                id,
                _ => NewKey(before, action),
                (_, queue) => Update(before, action, queue)
            );

        private static ConcurrentQueue<IUserEvent> NewKey(IUser user, UserAction type) =>
            Update(user, type, new ConcurrentQueue<IUserEvent>());

        private static ConcurrentQueue<IUserEvent> Update(IUser user, UserAction type,
            ConcurrentQueue<IUserEvent> events) {
            events.Enqueue(new UserEvent.Implementations.UserEvent(user, type));
            return events;
        }
    }
}
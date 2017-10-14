using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Botbin.GameTracking.UserEvent.Enums;
using Botbin.UserTracking.UserEvent;
using static Botbin.GameTracking.UserEvent.Enums.UserAction;

namespace Botbin.UserTracking.Implementations {
    internal class ConcurrentInMemoryUserTracker : IUserTracker {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;

        public ConcurrentInMemoryUserTracker() =>
            _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task Listen(IUser before, IUser after) {
            var id = before.Id;
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var logIn = before.Status == UserStatus.Offline && after.Status == UserStatus.Online;
            var logOff = before.Status == UserStatus.Online && after.Status == UserStatus.Offline;
            var startGame = !before.Game.HasValue && after.Game.HasValue;

            if (logIn) Save(after, id, LogIn);
            if (logOff) Save(after, id, LogOff);
            if (startGame) Save(after, id, StartGame);
            if (quitGame) Save(before, id, QuitGame);
            return Task.CompletedTask;
        }

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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.GameTracking.UserEvent;
using Botbin.GameTracking.UserEvent.Enums;
using Discord;

namespace Botbin.GameTracking.Implementations {
    /// <inheritdoc />
    internal class GameTracker : IGameTracker {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;

        public GameTracker() => _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();

        public IEnumerable<IUserEvent> UserEventsById(ulong id) =>
            _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(p => p.Value);

        public Task Listen(IUser before, IUser after) {
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            var id = before.Id;
            if (startGame) Save(after, id, UserAction.StartGame);
            else if (quitGame) Save(before, id, UserAction.QuitGame);

            return Task.CompletedTask;
        }

        private void Save(IUser before, ulong id, UserAction action) =>
            _dictionary.AddOrUpdate(
                id,
                _ => NewKey(before, UserAction.QuitGame),
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
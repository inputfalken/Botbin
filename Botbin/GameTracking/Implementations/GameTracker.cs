using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Botbin.GameTracking.UserEvent;
using Botbin.GameTracking.UserEvent.Enums;
using Discord;

namespace Botbin.GameTracking.Implementations {
    internal class GameTracker : IUserListener, IGameTracker {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>> _dictionary;

        public GameTracker() => _dictionary = new ConcurrentDictionary<ulong, ConcurrentQueue<IUserEvent>>();

        public IEnumerable<IUserEvent> UserEventsById(ulong id) => _dictionary.TryGetValue(id, out var result) ? result : Enumerable.Empty<IUserEvent>();

        public IEnumerable<IUserEvent> UserEvents() => _dictionary.SelectMany(pair => pair.Value);

        public Task Listen(IUser before, IUser after) {
            var quitGame = before.Game.HasValue && !after.Game.HasValue;
            var startGame = !before.Game.HasValue && after.Game.HasValue;
            var id = before.Id;

            if (startGame)
                _dictionary.AddOrUpdate(
                    id,
                    _ => Add(after, UserAction.StartGame),
                    (_, queue) => Update(after, UserAction.StartGame, queue)
                );
            else if (quitGame)
                _dictionary.AddOrUpdate(
                    id,
                    _ => Add(before, UserAction.QuitGame),
                    (_, queue) => Update(before, UserAction.QuitGame, queue)
                );
            return Task.CompletedTask;
        }

        private static ConcurrentQueue<IUserEvent> Add(IUser user, UserAction type) {
            var concurrentQueue = new ConcurrentQueue<IUserEvent>();
            concurrentQueue.Enqueue(new UserEvent.Implementations.UserEvent(user, type));
            return concurrentQueue;
        }

        private static ConcurrentQueue<IUserEvent> Update(IUser user, UserAction type,
            ConcurrentQueue<IUserEvent> events) {
            events.Enqueue(new UserEvent.Implementations.UserEvent(user, type));
            return events;
        }
    }
}